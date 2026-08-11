# Restaurant Management Platform

A web-based platform for managing restaurants, including menu management, order processing, payment tracking, user roles, and reviews. Designed for efficient restaurant operations and administration.

## Public URL

**Live Application:** https://krpetg-restaurantorder-monolith.proxy.itcollege.ee/

## Technology Stack

- **Backend:** ASP.NET Core with Entity Framework Core
- **Database:** (Default: SQL Server, containerized with Docker)
- **Frontend:** ASP.NET MVC (Razor Pages) with HTMX and Alpine.js
- **Authentication:** ASP.NET Identity with role-based authorization
- **Architecture:** Modular, supports multi-role access (Admin, Restaurant User, etc.)

## Key Features

- Menu, category, and item management
- Order and payment processing
- Restaurant and user administration
- Review and feedback system
- Role-based access control
- Dockerized development and deployment

---

# Tech notes

Based on .NET. Everything is running in docker (db and webapp).


### Install or update tooling for .net

~~~bash
dotnet tool update -g dotnet-ef
dotnet tool update -g dotnet-aspnet-codegenerator
dotnet tool update -g Microsoft.Web.LibraryManager.Cli
~~~


## Generate EF Core database migration

Run from solution folder.

~~~bash
dotnet ef migrations --project App.DAL.EF --startup-project WebApp add Initial
dotnet ef database   --project App.DAL.EF --startup-project WebApp drop
dotnet ef migrations --project App.DAL.EF --startup-project WebApp remove
dotnet ef database   --project App.DAL.EF --startup-project WebApp update
~~~


## Generate MVC controllers

Install from nuget:
- Microsoft.VisualStudio.Web.CodeGeneration.Design
- Microsoft.EntityFrameworkCore.SqlServer

Run from inside the WebApp directory.

MVC Web Controllers (disable global warnings as errors - otherwise only one controller will be generated, then compile starts to fail)
OLD! (LOOK AT NEW)
~~~bash
cd WebApp

dotnet aspnet-codegenerator controller -name AddressesController -m  Address -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenusController -m  Menu -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenuCategoriesController -m  MenuCategory -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenuItemsController -m  MenuItem -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OpeningHoursController -m  OpeningHour -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OrdersController -m  Order -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OrderRowsController -m  OrderRow -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PaymentsController -m  Payment -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RestaurantsController -m  Restaurant -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RestaurantUsersController -m  RestaurantUser -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ReviewsController -m  Review -actions -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
~~~

NEW!
~~~bash
cd WebApp

dotnet aspnet-codegenerator controller -name AddressesController -actions -m App.Domain.Address -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenusController -actions -m App.Domain.Menu -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenuCategoriesController -actions -m App.Domain.MenuCategory -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name MenuItemsController -actions -m App.Domain.MenuItem -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OpeningHoursController -actions -m App.Domain.OpeningHour -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OrdersController [Delete.cshtml](WebApp/Views/OrderRows/Delete.cshtml)-actions -m App.Domain.Order -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name OrderRowsController -actions -m App.Domain.OrderRow -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name PaymentsController -actions -m App.Domain.Payment -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RestaurantsController -actions -m App.Domain.Restaurant -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name RestaurantUsersController -actions -m App.Domain.RestaurantUser -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f
dotnet aspnet-codegenerator controller -name ReviewsController -actions -m App.Domain.Review -dc AppDbContext -outDir Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

# use area

dotnet aspnet-codegenerator controller -name RefreshTokensController -actions -m App.Domain.Identity.AppRefreshToken -dc AppDbContext -outDir Areas/Admin/Controllers --useDefaultLayout --useAsyncActions --referenceScriptLibraries -f

~~~

Api controllers
~~~bash
dotnet aspnet-codegenerator controller -name AddressesController -m  App.Domain.Address -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name MenusController -m  App.Domain.Menu -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name MenuCategoriesController -m  App.Domain.MenuCategory -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name MenuItemsController -m  App.Domain.MenuItem -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name OpeningHoursController -m  App.Domain.OpeningHour -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name OrdersController -m  App.Domain.Order -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name OrderRowsController -m  App.Domain.OrderRow -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name PaymentsController -m  App.Domain.Payment -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name RestaurantsController -m  App.Domain.Restaurant -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name RestaurantUsersController -m  App.Domain.RestaurantUser -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
dotnet aspnet-codegenerator controller -name ReviewsController -m  App.Domain.Review -dc AppDbContext -outDir ApiControllers -api --useAsyncActions -f
~~~

## Docker

~~~bash
docker buildx build --progress=plain --force-rm --push -t akaver/webapp:latest . 

# multiplatform build on apple silicon
# https://docs.docker.com/build/building/multi-platform/
docker buildx create --name mybuilder --bootstrap --use
docker buildx build --platform linux/amd64 -t akaver/webapp:latest --push .
~~~


## Prompt engineering for Cursor
Context: Article.cs, Organization.cs, AppDbContext.cs
~~~xml
<purpose>
   Article must be categorized
</purpose>

<instructions>
   <instruction>Generate new model class in App.Domain project. named ArticleCategroy</instruction>
   <instruction>Add optional 1:m relationship between Article and ArticleCategroy</instruction>
   <instruction>Category is owned by Organization</instruction>
   <instruction>Use LangStr for CategoryDisplayName</instruction>
   <instruction>Use string for CategoryName, limited to 64 characters</instruction>
   <instruction>ArticleCategory must also have optional collection navigation property to Article</instruction>
   <instruction>Do not modify entity relationship rules in OnModelCreating</instruction>
</instructions>
~~~

Optimize generated controller - use viewmodels

Context: ArticleCategoryController.cs, Organization.cs
~~~xml
<purpose>
   Optimize  controller and associated views - use viewmodels and nameof. Entity is in ArticleCategory.cs
</purpose>

<instructions>
   <instruction>Controllers, Views and Viemodels folders and files are located in Admin area - WebApp/Areas/Admin</instruction>
   <instruction>Generate and use viewmodel for entity</instruction>
   <instruction>Do not copy properties over from entity - use it directly inside viewmodel</instruction>
   <instruction>Avoid using ViewBag and ViewData</instruction>
   <instruction>Use nameof function in selectlist creation</instruction>
   <instruction>Extract selectlist creation to separate method, order data by selectlist value field. Support also to set specific selected value - needed in Edit methods</instruction>
   <instruction>Selectlist creation methods should have default value as null for selected value. Example: (Guid? selectedValue = null)</instruction>
   <instruction>Use async methods with await as needed</instruction>
   <instruction>Update views to use the viewmodel</instruction>
   <instruction>In viewmodels use [ValidateNever] attribute for selectlists</instruction>
</instructions>
~~~




## JS Libs

Add htmx and alpine to js libs.
~~~bash
libman install htmx.org --files dist/htmx.min.js 
libman install alpinejs --files dist/cdn.min.js 
~~~

## Generate identity UI

Install Microsoft.VisualStudio.Web.CodeGeneration.Design to WebApp.  
Run from inside the WebApp directory.

~~~bash
dotnet aspnet-codegenerator identity -dc DAL.App.EF.AppDbContext -f  
~~~
# restaurant-management-modular-monolith
