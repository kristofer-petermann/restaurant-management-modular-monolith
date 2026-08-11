# Modular Monolith Architecture Guide

This solution is a modular monolith: one deployable ASP.NET Core application hosts multiple business modules in-process, while each module owns its own domain model, persistence, service registration, migrations, and integration surface.

The goal is to keep deployment simple like a monolith, but structure the code so business capabilities are isolated like separate services.

## Core Idea

A module owns a business capability.

Other code should not reach into a module's database tables, EF entities, repositories, internal services, or implementation details. Other code should communicate with a module through one of these mechanisms:

- Public application interfaces exposed by the module.
- Contracts in `Shared.Contracts`.
- In-process events through `Mediator`.
- HTTP/Razor/API endpoints exposed by `WebApp`.

This gives the project clear ownership boundaries without requiring separate deployments.

## Solution-Level Structure

The solution is organized around modules and shared infrastructure:

```text
WebApp
Modules.Identity
Modules.ListItem
Shared.Contracts
Base.*
```

`WebApp` is the executable host. It starts the ASP.NET Core app, configures middleware, registers modules, and exposes MVC/API endpoints.

`Modules.*` projects contain business capabilities. Each module is a separate project and owns its own internal layers.

`Shared.Contracts` contains stable cross-module contracts and events.

`Base.*` projects contain reusable generic infrastructure such as base entities, repositories, services, Unit of Work contracts, and helpers.

## Module Structure

A module is usually structured like this:

```text
Modules.SomeFeature/
  SomeFeatureModuleExtensions.cs

  Domain/
    SomeEntity.cs

  Application/
    ISomeFeatureService.cs
    SomeFeatureService.cs
    Dto/
    Handlers/
    Services/

  Infrastructure/
    SomeFeatureDbContext.cs
    ISomeFeatureRepository.cs
    SomeFeatureRepository.cs
    ISomeFeatureUow.cs
    SomeFeatureUow.cs
    Dto/
    Mappers/
    Migrations/
    Seeding/
```

The project boundary is the main boundary. `Domain`, `Application`, and `Infrastructure` are folders inside the module project, not separate projects. Use `internal` implementation classes to hide module internals from the rest of the monolith.

## Composition Root

`WebApp` is the composition root. It references module projects and registers them during startup.

Each module exposes a single registration extension method:

```csharp
builder.Services.AddIdentityModule(connectionString);
builder.Services.AddListItemModule(connectionString);
```

This keeps startup readable and prevents `WebApp` from manually wiring every repository, DbContext, mapper, and service from every module.

## Persistence

Each module owns its own EF Core `DbContext`.

Even when modules share the same physical database, each module should use:

- Its own `DbContext`.
- Its own schema.
- Its own migrations.
- Its own seeding logic.
- Its own persistence DTOs and mappers where needed.

Example:

```csharp
builder.HasDefaultSchema("orders");
```

This avoids one global `AppDbContext` becoming a shared model for the whole application.

## Database Boundary Rules

Do not create normal EF navigation properties across module boundaries.

Prefer soft references:

```csharp
public Guid UserId { get; set; }
```

Avoid direct cross-module references:

```csharp
public AppUser User { get; set; } = default!;
```

If one module needs information from another module, it should use a public module API or a contract from `Shared.Contracts`.

This keeps modules independent at the model level. It also makes future extraction into services much easier, because there are fewer database-level and object-model dependencies to untangle.

## Shared Contracts

`Shared.Contracts` contains the stable contracts used between modules.

Good candidates:

- Module API interfaces.
- Integration events.
- Message contracts.
- Small shared abstractions needed for communication.

Bad candidates:

- EF entities.
- Repository interfaces.
- Internal module DTOs.
- Business logic.
- General utility dumping ground code.

Keep this project small. If everything goes into `Shared.Contracts`, the architecture quietly becomes a distributed shared model instead of a modular monolith.

## Cross-Module Communication

Use direct module APIs when the caller needs an immediate answer.

Example:

```csharp
var displayName = await identity.GetUserDisplayNameAsync(userId, ct);
```

Use events when another module should react to something, but the publisher should not know or care who reacts.

Example:

```csharp
await publisher.Publish(new ListItemCreatedEvent(id, ownerId, description), ct);
```

The practical rule:

- Query needed now: use a module API.
- Side effect or notification: publish an event.

## Application Flow

A typical request should flow like this:

```text
HTTP request
  -> WebApp controller
  -> Module application interface
  -> Module application service
  -> Module Unit of Work / repository
  -> Module DbContext
  -> Module database schema
```

Controllers should orchestrate HTTP concerns, authentication, model binding, and response mapping. Business rules should live inside modules.

## Layer Responsibilities

`WebApp`:

- Hosts the ASP.NET Core process.
- Owns controllers, Razor views, API routes, auth setup, Swagger, localization, middleware, and app startup.
- Registers modules through extension methods.

`Modules.X.Domain`:

- Owns module entities and domain-level model.
- Should usually be internal to the module.

`Modules.X.Application`:

- Owns use cases, public service interfaces, application DTOs, handlers, and module-facing operations.

`Modules.X.Infrastructure`:

- Owns EF Core DbContext, repositories, migrations, seeding, persistence DTOs, and persistence mappers.

`Shared.Contracts`:

- Owns cross-module interfaces, events, and message contracts.

`Base.*`:

- Owns reusable generic infrastructure shared across modules.

## How To Convert Another Architecture To This Style

1. Identify business capabilities.

   Convert technical folders like `Services`, `Repositories`, and `Models` into feature modules like `Identity`, `Orders`, `Billing`, `Inventory`, or `ListItem`.

2. Create one project per module.

   Use `Modules.FeatureName.csproj`. Do not create one project per technical layer unless the codebase has a strong reason to do so.

3. Move feature-owned entities into the module.

   Put them under `Modules.FeatureName/Domain`.

4. Give each module its own DbContext.

   Avoid one global `AppDbContext` containing every table.

5. Give each module its own database schema.

   Example:

   ```csharp
   builder.HasDefaultSchema("orders");
   ```

6. Move migrations into the owning module.

   Each module evolves its own schema.

7. Replace cross-module navigation properties with IDs.

   Use soft references like `UserId`, `OrderId`, or `CustomerId`.

8. Remove cross-module EF foreign keys unless there is a deliberate reason.

   Prefer module ownership over relational convenience.

9. Define cross-module contracts in `Shared.Contracts`.

   Put only stable interfaces, events, and messages there.

10. Register each module through one extension method.

    Example:

    ```csharp
    builder.Services.AddOrdersModule(connectionString);
    ```

11. Keep the web app as a thin host.

    Controllers may orchestrate requests, but business rules should live in modules.

12. Use events for side effects.

    Examples: `OrderPlacedEvent`, `UserRegisteredEvent`, `InvoicePaidEvent`.

13. Use direct module APIs for queries.

    Example: `ICustomerModuleApi.GetCustomerDisplayNameAsync(customerId)`.

14. Keep implementation types internal.

    Expose only what callers truly need.

15. Add tests around module boundaries.

    Test services, mappers, repositories, controllers, and cross-module events separately.

## Template For A New Module

```text
Modules.Orders/
  OrdersModuleExtensions.cs

  Domain/
    Order.cs
    OrderLine.cs

  Application/
    IOrderService.cs
    OrderService.cs
    Dto/
      OrderBllDto.cs
    Handlers/
      PaymentCapturedHandler.cs

  Infrastructure/
    OrdersDbContext.cs
    IOrderRepository.cs
    OrderRepository.cs
    IOrderUow.cs
    OrderUow.cs
    Dto/
      OrderDalDto.cs
    Mappers/
      OrderDalMapper.cs
    Migrations/
    Seeding/
```

Register it in `WebApp/Program.cs`:

```csharp
builder.Services.AddOrdersModule(connectionString);
```

## Practical Rules

Do not let modules reference each other directly unless the reference is to `Shared.Contracts`.

Do not inject another module's DbContext.

Do not query another module's tables from your module.

Do not share EF entities between modules.

Do not put business logic in `WebApp`.

Do not make `Shared.Contracts` a dumping ground for common code.

Do expose a narrow module API when another module needs data.

Do publish events when another module may react.

Do keep each module's migrations, seeding, DbContext, and DI registration inside the module.

## This Project's Exact Architecture

This project currently contains a modular monolith built from these main projects:

```text
HWDemo.sln

WebApp/
WebApp.Tests/

Modules.Identity/
Modules.ListItem/
Shared.Contracts/

Base.Contracts/
Base.Domain/
Base.DAL.Contracts/
Base.DAL.EF/
Base.BLL.Contracts/
Base.BLL/
Base.Helpers/

App.Resources/
```

There are also older `App.*` folders in the repository output, but the active modular-monolith structure is centered on `WebApp`, `Modules.*`, `Shared.Contracts`, and `Base.*`.

### Host Project

`WebApp` is the only executable application. It is responsible for:

- ASP.NET Core startup.
- Middleware pipeline.
- MVC controllers.
- API controllers.
- Razor views.
- Authentication and JWT setup.
- Swagger.
- Localization.
- Module registration.
- Initial migration and seeding orchestration.

The module registration happens in `WebApp/Program.cs`:

```csharp
builder.Services.AddIdentityModule(connectionString);
builder.Services.AddListItemModule(connectionString);
builder.Services.AddMediator();
```

`WebApp` references:

```text
App.Resources
Base.Helpers
Modules.Identity
Modules.ListItem
Shared.Contracts
```

### Identity Module

Project:

```text
Modules.Identity/Modules.Identity.csproj
```

Structure:

```text
Modules.Identity/
  Application/
    Dto/
    Handlers/
    Services/
    IdentityModuleApi.cs
  Domain/
    AppUser.cs
    AppRole.cs
    AppRefreshToken.cs
  Infrastructure/
    AppIdentityDbContext.cs
    Migrations/
    Seeding/
  IdentityModuleExtensions.cs
```

Responsibilities:

- Owns users, roles, refresh tokens, and Identity-related persistence.
- Owns `AppIdentityDbContext`.
- Uses database schema `identity`.
- Registers ASP.NET Core Identity.
- Registers data protection key persistence.
- Registers `IIdentityModuleApi`.
- Registers refresh-token services.
- Handles selected cross-module events.

Registration:

```csharp
services.AddDbContext<AppIdentityDbContext>(...);
services.AddDataProtection().PersistKeysToDbContext<AppIdentityDbContext>();
services.AddIdentity<AppUser, AppRole>(...);
services.AddScoped<IIdentityModuleApi, IdentityModuleApi>();
services.AddScoped<IRefreshTokenService, RefreshTokenService>();
```

Database schema:

```csharp
builder.HasDefaultSchema("identity");
```

Public cross-module API:

```csharp
IIdentityModuleApi.GetUserDisplayNameAsync(Guid userId, CancellationToken ct)
```

Important note: `AppUser` is public because ASP.NET Identity types are consumed from `WebApp` by Identity UI and account controllers. This is a framework-driven exception to the usual preference for internal domain entities.

### ListItem Module

Project:

```text
Modules.ListItem/Modules.ListItem.csproj
```

Structure:

```text
Modules.ListItem/
  Application/
    Dto/
    Mappers/
    IListItemService.cs
    ListItemService.cs
  Domain/
    ListItem.cs
  Infrastructure/
    Dto/
    Mappers/
    Migrations/
    Seeding/
    ListItemDbContext.cs
    IListItemRepository.cs
    ListItemRepository.cs
    IListItemUow.cs
    ListItemUow.cs
  ListItemModuleExtensions.cs
```

Responsibilities:

- Owns list item domain logic and persistence.
- Owns `ListItemDbContext`.
- Uses database schema `list_items`.
- Owns ListItem migrations.
- Exposes `IListItemService` to the host app.
- Keeps repository, Unit of Work, persistence DTOs, and EF entities internal.

Registration:

```csharp
services.AddDbContext<ListItemDbContext>(...);
services.AddScoped<IListItemUow, ListItemUow>();
services.AddScoped<IListItemService>(ListItemServiceFactory.Create);
```

Database schema:

```csharp
builder.HasDefaultSchema("list_items");
```

Cross-module user ownership:

```csharp
internal sealed class ListItem : BaseEntityAppUser
```

The `AppUserId` is a soft foreign key. The module intentionally does not define an EF navigation property to `Identity.AppUser`.

### Shared Contracts

Project:

```text
Shared.Contracts/Shared.Contracts.csproj
```

Structure:

```text
Shared.Contracts/
  Identity/
    IIdentityModuleApi.cs
    Events/
      UserRegisteredEvent.cs
  ListItem/
    Events/
      ListItemCreatedEvent.cs
```

Responsibilities:

- Defines cross-module API contracts.
- Defines cross-module events.
- References `Mediator.Abstractions` for event contracts.

Current contracts:

```csharp
IIdentityModuleApi
UserRegisteredEvent
ListItemCreatedEvent
```

### Cross-Module Communication In This Project

`WebApp.Controllers.ListItemsController` uses `IIdentityModuleApi` to ask Identity for the current user's display name:

```csharp
ViewData["OwnerDisplayName"] =
    await _identity.GetUserDisplayNameAsync(ownerId, ct);
```

`WebApp.Controllers.ListItemsController` and `WebApp.ApiControllers.ListItemsController` publish `ListItemCreatedEvent` after creating a list item:

```csharp
await _publisher.Publish(
    new ListItemCreatedEvent(listItem.Id, ownerId, listItem.ItemDescription), ct);
```

`Modules.Identity.Application.Handlers.ListItemCreatedAuditHandler` handles that event:

```csharp
internal sealed class ListItemCreatedAuditHandler
    : INotificationHandler<ListItemCreatedEvent>
```

This means ListItem creation can trigger Identity-side audit behavior without the ListItem module knowing about the Identity handler.

### Request Flow For List Items

MVC flow:

```text
Browser
  -> WebApp.Controllers.ListItemsController
  -> Modules.ListItem.Application.IListItemService
  -> Modules.ListItem.Application.ListItemService
  -> Modules.ListItem.Infrastructure.IListItemUow
  -> Modules.ListItem.Infrastructure.IListItemRepository
  -> Modules.ListItem.Infrastructure.ListItemDbContext
  -> PostgreSQL schema: list_items
```

API flow:

```text
HTTP client
  -> WebApp.ApiControllers.ListItemsController
  -> Modules.ListItem.Application.IListItemService
  -> Modules.ListItem.Application.ListItemService
  -> Modules.ListItem.Infrastructure.ListItemUow
  -> Modules.ListItem.Infrastructure.ListItemRepository
  -> Modules.ListItem.Infrastructure.ListItemDbContext
  -> PostgreSQL schema: list_items
```

Identity flow:

```text
HTTP client
  -> WebApp.ApiControllers.Identity.AccountController
  -> ASP.NET Core Identity services
  -> Modules.Identity.Infrastructure.AppIdentityDbContext
  -> PostgreSQL schema: identity
```

### Base Infrastructure

The `Base.*` projects provide generic building blocks used by the modules:

```text
Base.Contracts
Base.Domain
Base.DAL.Contracts
Base.DAL.EF
Base.BLL.Contracts
Base.BLL
Base.Helpers
```

Examples:

- `BaseEntity`
- `BaseEntityAppUser`
- `IBaseRepository`
- `IBaseUOW`
- `BaseRepository`
- `BaseUOW`
- `IBaseService`
- `BaseService`
- `IBaseMapper`
- identity helper methods

The ListItem module builds its service, repository, and Unit of Work on top of these base abstractions.

### Testing Project

`WebApp.Tests` contains tests across several levels:

```text
WebApp.Tests/
  E2E/
  Integration/
  Unit/
  Helpers/
```

The tests cover API behavior, MVC behavior, DAL behavior, mappers, BLL services, domain behavior, and browser-level flows.

The module projects expose internals to `WebApp.Tests` through `InternalsVisibleTo`, allowing tests to inspect internal module types without making those types public in production code.

### Current Boundary Exceptions

There are a few pragmatic exceptions:

- `WebApp` references module projects directly because this is an in-process monolith.
- Identity domain types such as `AppUser` are public because ASP.NET Core Identity and the `WebApp` account controller need to consume them.
- `InternalsVisibleTo` is used so `WebApp`, `WebApp.Tests`, and dynamic proxy generation can access selected internal module types.
- Both modules use the same physical database connection string, but separate schemas and DbContexts.

These are acceptable for this architecture as long as module data ownership remains clear.
