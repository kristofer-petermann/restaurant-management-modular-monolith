# Transition To Modular Monolith

## Context

This project is already the copied phase-2 project. The goal is to refactor this existing ASP.NET Core solution from the current Clean/Onion architecture into a modular monolith architecture.

The required modules are:

- `Modules.Identity`
- `Modules.Restaurants`
- `Modules.Orders`

The modules must communicate through mediator/contracts. There must be no direct project references between modules.

The target architecture follows the rules described in `MONOLITH_ARCHITECTURE.md`.

## Current Starting Point

The current solution is still organized around phase-2 layered projects:

```text
App.Domain
App.BLL
App.BLL.Contracts
App.BLL.DTO
App.DAL.EF
App.DAL.Contracts
App.DAL.DTO
App.DTO
WebApp
WebApp.Tests
Base.*
App.Resources
```

The refactor should happen in this repository. Do not create another project copy.

## Target Solution Structure

The solution should move toward this structure:

```text
WebApp/
  Controllers/
  ApiControllers/
  ViewModels/
  Program.cs

Modules.Identity/
  IdentityModuleExtensions.cs
  Domain/
  Application/
  Infrastructure/

Modules.Restaurants/
  RestaurantsModuleExtensions.cs
  Domain/
  Application/
  Infrastructure/

Modules.Orders/
  OrdersModuleExtensions.cs
  Domain/
  Application/
  Infrastructure/

Shared.Contracts/
  Identity/
  Restaurants/
  Orders/
  Events/

Base.Contracts/
Base.Domain/
Base.DAL.Contracts/
Base.DAL.EF/
Base.BLL.Contracts/
Base.BLL/
Base.Helpers/

App.Resources/
WebApp.Tests/
```

`WebApp` remains the only executable application.

## Project Reference Rules

Allowed references:

```text
WebApp -> Modules.Identity
WebApp -> Modules.Restaurants
WebApp -> Modules.Orders
WebApp -> Shared.Contracts
WebApp -> Base.*
WebApp -> App.Resources

Modules.Identity -> Shared.Contracts
Modules.Identity -> Base.*

Modules.Restaurants -> Shared.Contracts
Modules.Restaurants -> Base.*

Modules.Orders -> Shared.Contracts
Modules.Orders -> Base.*
```

Forbidden references:

```text
Modules.Identity -> Modules.Restaurants
Modules.Identity -> Modules.Orders
Modules.Restaurants -> Modules.Identity
Modules.Restaurants -> Modules.Orders
Modules.Orders -> Modules.Identity
Modules.Orders -> Modules.Restaurants
```

Modules must not inject another module's `DbContext`, repositories, services, or EF entities.

## Module Ownership

### Identity Module

The Identity module owns:

```text
AppUser
AppRole
AppUserRole
RestaurantUser
EUserRoleInRestaurant
```

Responsibilities:

- ASP.NET Core Identity user and role model.
- Authentication-related persistence.
- Restaurant user membership and restaurant-specific roles.
- User display-name and permission lookup APIs.
- Identity-related mediator handlers.

Recommended folders:

```text
Modules.Identity/
  IdentityModuleExtensions.cs

  Domain/
    AppUser.cs
    AppRole.cs
    AppUserRole.cs
    RestaurantUser.cs
    EUserRoleInRestaurant.cs

  Application/
    IIdentityModuleApi.cs
    IRestaurantUserService.cs
    Dto/
    Handlers/
    Services/

  Infrastructure/
    IdentityDbContext.cs
    Migrations/
    Seeding/
    Repositories/
    Mappers/
```

### Restaurants Module

The Restaurants module owns:

```text
Restaurant
Address
OpeningHour
Menu
MenuCategory
MenuItem
Review
```

Responsibilities:

- Restaurant data.
- Restaurant addresses.
- Opening hours.
- Menus, categories, and menu items.
- Reviews.
- Restaurant and menu validation for the Orders module through mediator handlers.

Recommended folders:

```text
Modules.Restaurants/
  RestaurantsModuleExtensions.cs

  Domain/
    Restaurant.cs
    Address.cs
    OpeningHour.cs
    Menu.cs
    MenuCategory.cs
    MenuItem.cs
    Review.cs

  Application/
    IRestaurantService.cs
    IAddressService.cs
    IOpeningHourService.cs
    IMenuService.cs
    IMenuCategoryService.cs
    IMenuItemService.cs
    IReviewService.cs
    Dto/
    Handlers/
    Services/

  Infrastructure/
    RestaurantsDbContext.cs
    Migrations/
    Seeding/
    Repositories/
    Mappers/
```

### Orders Module

The Orders module owns:

```text
Order
OrderRow
Payment
```

Responsibilities:

- Order creation and management.
- Order rows.
- Payment data.
- Order total calculation.
- Publishing order-related events.
- Querying restaurant/menu item snapshots through mediator/contracts instead of direct module references.

Recommended folders:

```text
Modules.Orders/
  OrdersModuleExtensions.cs

  Domain/
    Order.cs
    OrderRow.cs
    Payment.cs

  Application/
    IOrderService.cs
    IOrderRowService.cs
    IPaymentService.cs
    Dto/
    Handlers/
    Services/

  Infrastructure/
    OrdersDbContext.cs
    Migrations/
    Seeding/
    Repositories/
    Mappers/
```

## Shared Contracts

Create a `Shared.Contracts` project for stable cross-module contracts only.

Good candidates:

- Mediator requests.
- Mediator responses.
- Integration events.
- Narrow module API interfaces when needed.

Do not put these into `Shared.Contracts`:

- EF entities.
- Repository interfaces.
- Internal DTOs.
- Business logic.
- Generic utility code.

Suggested contracts:

```text
Shared.Contracts/
  Identity/
    GetUserDisplayNameQuery.cs
    ValidateRestaurantUserPermissionQuery.cs
    UserDisplayNameResponse.cs

  Restaurants/
    GetRestaurantSummaryQuery.cs
    GetMenuItemSnapshotQuery.cs
    ValidateRestaurantExistsQuery.cs
    ValidateMenuItemsQuery.cs
    RestaurantSummaryResponse.cs
    MenuItemSnapshotResponse.cs

  Orders/
    OrderCreatedEvent.cs
    OrderPaidEvent.cs

  Events/
    ReviewCreatedEvent.cs
    RestaurantUserAssignedEvent.cs
```

## Cross-Module Communication

Use mediator for communication between modules.

Use request/response contracts when the caller needs information immediately.

Example:

```text
Orders -> GetMenuItemSnapshotQuery -> Restaurants
Orders -> GetUserDisplayNameQuery -> Identity
Restaurants -> ValidateRestaurantUserPermissionQuery -> Identity
```

Use events when another module may react to something, but the publisher should not know who reacts.

Example:

```text
Orders publishes OrderCreatedEvent
Orders publishes OrderPaidEvent
Restaurants publishes ReviewCreatedEvent
Identity publishes RestaurantUserAssignedEvent
```

## Database Boundary Rules

Each module must own its own EF Core `DbContext`.

```text
Modules.Identity.Infrastructure.IdentityDbContext
Modules.Restaurants.Infrastructure.RestaurantsDbContext
Modules.Orders.Infrastructure.OrdersDbContext
```

Each module should use its own database schema:

```csharp
builder.HasDefaultSchema("identity");
builder.HasDefaultSchema("restaurants");
builder.HasDefaultSchema("orders");
```

Each module owns its own migrations:

```text
Modules.Identity/Infrastructure/Migrations
Modules.Restaurants/Infrastructure/Migrations
Modules.Orders/Infrastructure/Migrations
```

The old global `AppDbContext` should be removed after all module DbContexts are working.

## Entity Boundary Rules

Do not create EF navigation properties across module boundaries.

Use soft references by ID.

Good:

```csharp
public Guid UserId { get; set; }
public Guid RestaurantId { get; set; }
public Guid MenuItemId { get; set; }
```

Bad across modules:

```csharp
public AppUser User { get; set; } = default!;
public Restaurant Restaurant { get; set; } = default!;
public MenuItem MenuItem { get; set; } = default!;
```

Normal navigation properties are allowed inside one module.

For example, this is acceptable inside the Restaurants module:

```text
Restaurant -> Address
Restaurant -> OpeningHour
Restaurant -> Menu
Menu -> MenuCategory
MenuCategory -> MenuItem
Restaurant -> Review
```

## Important Entity Design Decisions

### RestaurantUser

`RestaurantUser` belongs to the Identity module because it represents a user role or membership inside a restaurant.

It should reference restaurants by ID only:

```csharp
public Guid RestaurantId { get; set; }
public Guid UserId { get; set; }
public EUserRoleInRestaurant Role { get; set; }
```

It must not have a direct `Restaurant` navigation property.

### Review

`Review` belongs to the Restaurants module.

It should reference users by ID only:

```csharp
public Guid UserId { get; set; }
public Guid RestaurantId { get; set; }
```

It must not have a direct `AppUser` navigation property.

If the UI needs the review author's name, the Restaurants module or WebApp should query Identity through mediator or a public identity module API.

### Order

`Order` belongs to the Orders module.

It should reference users and restaurants by ID only:

```csharp
public Guid UserId { get; set; }
public Guid RestaurantId { get; set; }
```

It must not have direct navigation properties to `AppUser` or `Restaurant`.

### OrderRow

`OrderRow` belongs to the Orders module.

It should not navigate directly to `MenuItem`.

Recommended shape:

```csharp
public Guid OrderId { get; set; }
public Guid MenuItemId { get; set; }
public string MenuItemName { get; set; } = default!;
public decimal UnitPrice { get; set; }
public int Quantity { get; set; }
```

This gives Orders a historical snapshot. If a menu item name or price changes later, old orders remain correct.

## Module Registration

Each module exposes one public registration extension method.

Identity:

```csharp
builder.Services.AddIdentityModule(connectionString);
```

Restaurants:

```csharp
builder.Services.AddRestaurantsModule(connectionString);
```

Orders:

```csharp
builder.Services.AddOrdersModule(connectionString);
```

`WebApp/Program.cs` should register:

```csharp
builder.Services.AddMediator();

builder.Services.AddIdentityModule(connectionString);
builder.Services.AddRestaurantsModule(connectionString);
builder.Services.AddOrdersModule(connectionString);
```

`WebApp` should not manually register every repository, mapper, DbContext, or service from each module.

## Controller Refactor

Controllers should stop depending on `IAppBLL`.

Replace this style:

```csharp
private readonly IAppBLL _bll;
```

With module-specific services:

```csharp
private readonly IRestaurantService _restaurantService;
private readonly IOrderService _orderService;
private readonly IPaymentService _paymentService;
```

Suggested ownership:

```text
RestaurantsController -> IRestaurantService
AddressesController -> IAddressService
OpeningHoursController -> IOpeningHourService
MenusController -> IMenuService
MenuCategoriesController -> IMenuCategoryService
MenuItemsController -> IMenuItemService
ReviewsController -> IReviewService

OrdersController -> IOrderService
OrderRowsController -> IOrderRowService
PaymentsController -> IPaymentService

RestaurantUsersController -> IRestaurantUserService
AccountController -> Identity services
```

Controllers remain in `WebApp`. Business rules move into modules.

## DTO Strategy

Avoid one global DTO project for everything.

Recommended split:

```text
WebApp DTOs/ViewModels
  HTTP request and response models.

Module Application DTOs
  Use-case models used by module services.

Module Infrastructure DTOs
  Persistence models if still needed by repository/mapping patterns.

Shared.Contracts DTOs
  Only mediator responses and stable cross-module messages.
```

Do not move all `App.DTO` models into `Shared.Contracts`.

## Migration Strategy

1. Stop adding new migrations to `App.DAL.EF`.
2. Create module DbContexts.
3. Move entity configurations into the owning module DbContext.
4. Create module migrations:

```text
Modules.Identity/Infrastructure/Migrations
Modules.Restaurants/Infrastructure/Migrations
Modules.Orders/Infrastructure/Migrations
```

5. Remove old `App.DAL.EF/Migrations` after all module migrations are verified.

## Suggested Implementation Order

### Phase 1: Add the New Projects

Create:

```text
Shared.Contracts
Modules.Identity
Modules.Restaurants
Modules.Orders
```

Add them to the solution.

Add references:

```text
WebApp -> Modules.*
WebApp -> Shared.Contracts
Modules.* -> Shared.Contracts
Modules.* -> Base.*
```

Do not add module-to-module references.

### Phase 2: Add Mediator Infrastructure

Register mediator in `WebApp/Program.cs`.

Add initial contracts in `Shared.Contracts`.

Start with contracts needed by Orders:

```text
GetMenuItemSnapshotQuery
ValidateRestaurantExistsQuery
ValidateMenuItemsQuery
```

Add identity-related contracts:

```text
GetUserDisplayNameQuery
ValidateRestaurantUserPermissionQuery
```

### Phase 3: Move Identity

Move Identity entities and related logic into `Modules.Identity`.

Create `IdentityDbContext`.

Move or recreate Identity migrations under the Identity module.

Add `IdentityModuleExtensions`.

Update `Program.cs` to call:

```csharp
builder.Services.AddIdentityModule(connectionString);
```

Update `AccountController` and `RestaurantUsersController`.

### Phase 4: Move Restaurants

Move these entities:

```text
Restaurant
Address
OpeningHour
Menu
MenuCategory
MenuItem
Review
```

Move matching repositories, services, DTOs, and mappers.

Create `RestaurantsDbContext`.

Use schema:

```text
restaurants
```

Add `RestaurantsModuleExtensions`.

Add mediator handlers for:

```text
GetRestaurantSummaryQuery
GetMenuItemSnapshotQuery
ValidateRestaurantExistsQuery
ValidateMenuItemsQuery
```

Update restaurant, address, menu, menu item, category, opening hour, and review controllers.

### Phase 5: Move Orders

Move these entities:

```text
Order
OrderRow
Payment
```

Move matching repositories, services, DTOs, and mappers.

Create `OrdersDbContext`.

Use schema:

```text
orders
```

Add `OrdersModuleExtensions`.

Update order creation to use mediator for restaurant/menu item validation and menu item snapshots.

Publish:

```text
OrderCreatedEvent
OrderPaidEvent
```

Update order, order row, and payment controllers.

### Phase 6: Remove the Old App Layer

After all controllers use module services and all module DbContexts work, remove references to:

```text
App.Domain
App.BLL
App.BLL.Contracts
App.BLL.DTO
App.DAL.EF
App.DAL.Contracts
App.DAL.DTO
App.DTO
```

Then remove the old projects from the solution if nothing references them.

### Phase 7: Tests and Verification

Add or update tests for:

```text
Module services
Controller actions
Mediator handlers
Order creation with menu item snapshot
Review creation with user ID only
Restaurant user permission checks
Module DbContext configuration
```

Verify:

- Solution builds.
- WebApp starts.
- Login/register still works.
- Restaurant CRUD works.
- Menu CRUD works.
- Review CRUD works.
- Order creation works.
- Payment flow works.
- No module references another module directly.
- No module queries another module's DbContext.
- No cross-module EF navigation properties remain.

## Final Acceptance Checklist

- [ ] `Modules.Identity` exists.
- [ ] `Modules.Restaurants` exists.
- [ ] `Modules.Orders` exists.
- [ ] `Shared.Contracts` exists.
- [ ] `WebApp` registers all modules through module extension methods.
- [ ] Mediator is registered.
- [ ] Modules do not reference each other directly.
- [ ] Each module has its own DbContext.
- [ ] Each module has its own database schema.
- [ ] Each module has its own migrations.
- [ ] Cross-module entity references use IDs instead of EF navigation properties.
- [ ] Controllers no longer depend on `IAppBLL`.
- [ ] Old `App.*` projects are no longer referenced.
- [ ] The solution builds.
- [ ] Tests pass or are updated for the new module boundaries.

