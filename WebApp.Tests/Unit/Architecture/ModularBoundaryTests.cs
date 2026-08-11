using System.Xml.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.Architecture;

public class ModularBoundaryTests
{
    private static readonly string Root = FindRepoRoot();

    [Fact]
    public void WebApp_Must_Not_Use_Module_Internal_Namespaces()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var sourceFiles = GetSourceAndRazorFiles(webAppDir);

        var forbidden = new[]
        {
            "Modules.Identity.Domain",
            "Modules.Identity.Infrastructure",
            "Modules.Restaurants.Domain",
            "Modules.Restaurants.Infrastructure",
            "Modules.Orders.Domain",
            "Modules.Orders.Infrastructure"
        };

        var violations = new List<string>();
        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{Path.GetRelativePath(Root, file)} -> {marker}");
            }
        }

        violations.Should().BeEmpty("WebApp should depend on module application/public APIs only.");
    }

    [Fact]
    public void WebApp_Must_Not_Use_Module_Application_Services()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var sourceFiles = GetSourceAndRazorFiles(webAppDir);

        var forbidden = new[]
        {
            "Modules.Identity.Application.Services",
            "Modules.Restaurants.Application.Services",
            "Modules.Orders.Application.Services"
        };

        var violations = new List<string>();
        foreach (var file in sourceFiles)
        {
            var content = File.ReadAllText(file);
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{Path.GetRelativePath(Root, file)} -> {marker}");
            }
        }

        violations.Should().BeEmpty("WebApp should consume module APIs, not module application services.");
    }

    [Fact]
    public void WebApp_Module_Application_Dto_Usages_Must_Be_Allowlisted()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var dtoNamespaceUsings = new[]
        {
            "using Modules.Identity.Application.Dto;",
            "using Modules.Restaurants.Application.Dto;"
        };

        var currentUsages = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(Root, file).Replace('\\', '/');
            foreach (var marker in dtoNamespaceUsings.Where(content.Contains))
            {
                currentUsages.Add($"{relativePath} -> {marker}");
            }
        }

        var allowlistPath = Path.Combine(Root, "WebApp.Tests", "Unit", "Architecture", "WebApp_ApplicationDto_Allowlist.txt");
        File.Exists(allowlistPath).Should().BeTrue($"allowlist file must exist: {allowlistPath}");

        var allowed = File.ReadAllLines(allowlistPath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);

        var newViolations = currentUsages
            .Where(u => !allowed.Contains(u))
            .OrderBy(x => x)
            .ToList();

        newViolations.Should().BeEmpty(
            "new WebApp dependencies on module application DTO namespaces are not allowed; " +
            "use module API/public contracts or explicitly approve and document transition entries.");
    }

    [Fact]
    public void Modules_Must_Not_Reference_Other_Modules_Directly()
    {
        var moduleProjects = new[]
        {
            Path.Combine(Root, "Modules.Identity", "Modules.Identity.csproj"),
            Path.Combine(Root, "Modules.Restaurants", "Modules.Restaurants.csproj"),
            Path.Combine(Root, "Modules.Orders", "Modules.Orders.csproj")
        };

        var moduleNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Modules.Identity",
            "Modules.Restaurants",
            "Modules.Orders"
        };

        var violations = new List<string>();

        foreach (var projectPath in moduleProjects)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectPath);
            var doc = XDocument.Load(projectPath);
            var refs = doc.Descendants()
                .Where(e => e.Name.LocalName == "ProjectReference")
                .Select(e => (string?)e.Attribute("Include"))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();

            foreach (var reference in refs)
            {
                var referencedProject = Path.GetFileNameWithoutExtension(reference);
                if (moduleNames.Contains(referencedProject) &&
                    !string.Equals(referencedProject, projectName, StringComparison.OrdinalIgnoreCase))
                {
                    violations.Add($"{projectName} -> {referencedProject}");
                }
            }
        }

        violations.Should().BeEmpty("modules must communicate via Shared.Contracts, not direct module references.");
    }

    [Fact]
    public void Orders_Application_Services_Must_Not_Use_DbContext_For_Crud_Queries()
    {
        var servicesDir = Path.Combine(Root, "Modules.Orders", "Application", "Services");
        var serviceFiles = Directory.GetFiles(servicesDir, "*.cs", SearchOption.TopDirectoryOnly);

        var forbiddenMarkers = new[]
        {
            "_dbContext.Orders",
            "_dbContext.OrderRows",
            "_dbContext.Payments"
        };

        var violations = new List<string>();
        foreach (var file in serviceFiles)
        {
            var content = File.ReadAllText(file);
            foreach (var marker in forbiddenMarkers.Where(content.Contains))
            {
                violations.Add($"{Path.GetRelativePath(Root, file)} -> {marker}");
            }
        }

        violations.Should().BeEmpty(
            "Orders application services should use repositories for Order/OrderRow/Payment CRUD and query access.");
    }

    [Fact]
    public void Orders_Module_Public_Surface_Must_Be_Intentional()
    {
        var moduleDir = Path.Combine(Root, "Modules.Orders");
        var csFiles = Directory.GetFiles(moduleDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"));

        var allowedPrefixes = new[]
        {
            "Modules.Orders/OrdersModuleExtensions.cs",
            "Modules.Orders/Domain/"
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("public class") && !content.Contains("public interface")) continue;

            var relativePath = Path.GetRelativePath(Root, file).Replace('\\', '/');
            if (allowedPrefixes.Any(relativePath.StartsWith))
            {
                continue;
            }

            violations.Add(relativePath);
        }

        violations.Should().BeEmpty("Orders implementation types should stay internal by default.");
    }

    [Fact]
    public void WebApp_Must_Not_Use_Legacy_Restaurants_Mutation_Methods()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var forbidden = new[]
        {
            ".AddRestaurant(",
            ".UpdateRestaurantAsync(",
            ".RemoveRestaurantAsync(",
            ".AddMenu(",
            ".UpdateMenuAsync(",
            ".RemoveMenuAsync(",
            ".AddMenuCategory(",
            ".UpdateMenuCategoryAsync(",
            ".RemoveMenuCategoryAsync(",
            ".AddMenuItem(",
            ".UpdateMenuItemAsync(",
            ".RemoveMenuItemAsync(",
            ".AddAddress(",
            ".UpdateAddressAsync(",
            ".RemoveAddressAsync(",
            ".AddOpeningHour(",
            ".UpdateOpeningHourAsync(",
            ".RemoveOpeningHourAsync(",
            ".AddReview(",
            ".UpdateReviewAsync(",
            ".RemoveReviewAsync("
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(Root, file);
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{relativePath} -> {marker}");
            }
        }

        violations.Should().BeEmpty("WebApp should use Restaurants use-case methods instead of legacy CRUD-style mutators.");
    }

    [Fact]
    public void WebApp_Must_Not_Create_Reviews_Through_Legacy_Method()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains(".CreateReviewAsync("))
            {
                violations.Add(Path.GetRelativePath(Root, file).Replace('\\', '/'));
            }
        }

        violations.Should().BeEmpty("WebApp should create reviews via PublishReviewAsync use-case.");
    }

    [Fact]
    public void WebApp_Legacy_Order_Create_Usages_Must_Be_Confined_To_Allowlist()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var currentUsages = csFiles
            .Where(f => File.ReadAllText(f).Contains(".CreateOrderAsync("))
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(x => x)
            .ToList();

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "WebApp/Controllers/OrdersController.cs",
            "WebApp/Areas/Admin/Controllers/OrdersController.cs"
        };

        var violations = currentUsages.Where(x => !allowed.Contains(x)).ToList();
        violations.Should().BeEmpty("legacy CreateOrderAsync should not spread to new files.");
    }

    [Fact]
    public void WebApp_Legacy_OrderRow_Create_Usages_Must_Be_Confined_To_Allowlist()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var currentUsages = csFiles
            .Where(f => File.ReadAllText(f).Contains(".CreateOrderRowAsync("))
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(x => x)
            .ToList();

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "WebApp/Controllers/OrderRowsController.cs",
            "WebApp/Areas/Admin/Controllers/OrderRowsController.cs",
            "WebApp/ApiControllers/OrderRowsController.cs"
        };

        var violations = currentUsages.Where(x => !allowed.Contains(x)).ToList();
        violations.Should().BeEmpty("legacy CreateOrderRowAsync should not spread to new files.");
    }

    [Fact]
    public void WebApp_Legacy_Payment_Create_Usages_Must_Be_Confined_To_Allowlist()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var currentUsages = csFiles
            .Where(f => File.ReadAllText(f).Contains(".CreatePaymentAsync("))
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(x => x)
            .ToList();

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "WebApp/Controllers/PaymentsController.cs",
            "WebApp/Areas/Admin/Controllers/PaymentsController.cs",
            "WebApp/ApiControllers/PaymentsController.cs"
        };

        var violations = currentUsages.Where(x => !allowed.Contains(x)).ToList();
        violations.Should().BeEmpty("legacy CreatePaymentAsync should not spread to new files.");
    }

    [Fact]
    public void WebApp_Legacy_MenuItem_Mutation_Usages_Must_Be_Confined_To_Allowlist()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var mutationMarkers = new[]
        {
            ".CreateMenuItemAsync(",
            ".UpdateMenuItemDetailsAsync(",
            ".DeleteMenuItemAsync("
        };

        var currentUsages = csFiles
            .Where(f => mutationMarkers.Any(marker => File.ReadAllText(f).Contains(marker)))
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(x => x)
            .ToList();

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "WebApp/Controllers/MenuItemsController.cs",
            "WebApp/Areas/Admin/Controllers/MenuItemsController.cs",
            "WebApp/ApiControllers/MenuItemsController.cs"
        };

        var violations = currentUsages.Where(x => !allowed.Contains(x)).ToList();
        violations.Should().BeEmpty("legacy MenuItem mutation methods should not spread to new files.");
    }

    [Fact]
    public void WebApp_Legacy_Review_Mutation_Usages_Must_Be_Confined_To_Allowlist()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var mutationMarkers = new[]
        {
            ".UpdateReviewDetailsAsync(",
            ".DeleteReviewAsync("
        };

        var currentUsages = csFiles
            .Where(f => mutationMarkers.Any(marker => File.ReadAllText(f).Contains(marker)))
            .Select(f => Path.GetRelativePath(Root, f).Replace('\\', '/'))
            .OrderBy(x => x)
            .ToList();

        var allowed = new HashSet<string>(StringComparer.Ordinal)
        {
            "WebApp/Controllers/ReviewsController.cs",
            "WebApp/Areas/Admin/Controllers/ReviewsController.cs",
            "WebApp/ApiControllers/ReviewsController.cs"
        };

        var violations = currentUsages.Where(x => !allowed.Contains(x)).ToList();
        violations.Should().BeEmpty("legacy Review mutation methods should not spread to new files.");
    }

    [Fact]
    public void WebApp_Must_Not_Call_Restaurants_SaveChanges()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var lines = content.Split('\n');

            var restaurantsApiNames = Regex.Matches(content, @"IRestaurantsModuleApi\s+([A-Za-z_][A-Za-z0-9_]*)")
                .Select(m => m.Groups[1].Value)
                .ToHashSet(StringComparer.Ordinal);
            if (restaurantsApiNames.Count == 0) continue;

            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var apiName in restaurantsApiNames)
                {
                    if (lines[i].Contains($"{apiName}.SaveChangesAsync("))
                    {
                        violations.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}");
                    }
                }
            }
        }

        violations.Should().BeEmpty("Restaurants module API should persist changes internally via use-case methods.");
    }

    [Fact]
    public void Restaurants_Module_Public_Surface_Must_Be_Intentional()
    {
        var moduleDir = Path.Combine(Root, "Modules.Restaurants");
        var csFiles = Directory.GetFiles(moduleDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"));

        var allowedPrefixes = new[]
        {
            "Modules.Restaurants/Application/Dto/",
            "Modules.Restaurants/RestaurantsModuleExtensions.cs",
            "Modules.Restaurants/Domain/"
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("public class") && !content.Contains("public interface")) continue;

            var relativePath = Path.GetRelativePath(Root, file).Replace('\\', '/');
            if (allowedPrefixes.Any(relativePath.StartsWith))
            {
                continue;
            }

            violations.Add(relativePath);
        }

        violations.Should().BeEmpty("Restaurants implementation types should stay internal by default.");
    }

    [Fact]
    public void WebApp_ModuleApi_Aliases_Must_Point_To_SharedContracts()
    {
        var file = Path.Combine(Root, "WebApp", "ModuleApiUsings.cs");
        File.Exists(file).Should().BeTrue("WebApp must declare module API aliases.");

        var content = File.ReadAllText(file);
        content.Should().NotContain("= Modules.", "WebApp must not alias module APIs to module implementation namespaces.");
        content.Should().Contain("Shared.Contracts.Identity.IIdentityModuleApi");
        content.Should().Contain("Shared.Contracts.Orders.IOrdersModuleApi");
        content.Should().Contain("Shared.Contracts.Restaurants.IRestaurantsModuleApi");
    }

    [Fact]
    public void ModuleApi_Interfaces_Must_Not_Live_In_Module_Implementation_Projects()
    {
        var moduleDirs = new[]
        {
            Path.Combine(Root, "Modules.Identity"),
            Path.Combine(Root, "Modules.Restaurants"),
            Path.Combine(Root, "Modules.Orders")
        };

        var moduleFiles = moduleDirs
            .SelectMany(dir => Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var violations = new List<string>();
        foreach (var file in moduleFiles)
        {
            var content = File.ReadAllText(file);
            if (!Regex.IsMatch(content, @"\b(public|internal)\s+interface\s+I[A-Za-z0-9_]*ModuleApi\b"))
            {
                continue;
            }

            violations.Add(Path.GetRelativePath(Root, file).Replace('\\', '/'));
        }

        violations = violations.OrderBy(x => x).ToList();

        violations.Should().BeEmpty("module API interfaces must be defined in Shared.Contracts.");
    }

    [Fact]
    public void Program_Must_Use_Central_Data_Setup_Orchestration()
    {
        var programPath = Path.Combine(Root, "WebApp", "Program.cs");
        File.Exists(programPath).Should().BeTrue("Program.cs must exist.");

        var content = File.ReadAllText(programPath);
        content.Should().Contain("await SetupAppDataAsync(app, app.Configuration, connectionString);");
        content.Should().Contain("static async Task SetupAppDataAsync(");
    }

    [Fact]
    public void Program_Must_Not_Call_Legacy_Module_Initializers_Directly()
    {
        var programPath = Path.Combine(Root, "WebApp", "Program.cs");
        File.Exists(programPath).Should().BeTrue("Program.cs must exist.");

        var content = File.ReadAllText(programPath);
        content.Should().NotContain("InitializeIdentityModuleAsync(");
        content.Should().NotContain("InitializeRestaurantsModuleAsync(");
        content.Should().NotContain("InitializeOrdersModuleAsync(");
    }

    [Fact]
    public void WebApp_Must_Not_Use_Legacy_Identity_Mutation_Methods()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var forbidden = new[]
        {
            ".AddRestaurantUser(",
            ".UpdateRestaurantUserAsync(",
            ".RemoveRestaurantUserAsync("
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(Root, file);
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{relativePath} -> {marker}");
            }
        }

        violations.Should().BeEmpty("WebApp should use Identity use-case methods instead of legacy CRUD-style mutators.");
    }

    [Fact]
    public void WebApp_Must_Not_Use_Identity_RestaurantUser_Crud_ModuleApi_Methods()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var forbidden = new[]
        {
            ".GetRestaurantUsersAsync(",
            ".FindRestaurantUserAsync(",
            ".FindRestaurantUserEntityAsync(",
            ".CreateRestaurantUserAsync(",
            ".UpdateRestaurantUserDetailsAsync(",
            ".DeleteRestaurantUserAsync("
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(Root, file).Replace('\\', '/');
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{relativePath} -> {marker}");
            }
        }

        violations.Should().BeEmpty(
            "RestaurantUser CRUD calls through IIdentityModuleApi are deprecated; use Identity commands/queries.");
    }

    [Fact]
    public void WebApp_Controllers_Must_Not_Depend_On_RestaurantUserDto()
    {
        var controllerDirs = new[]
        {
            Path.Combine(Root, "WebApp", "Controllers"),
            Path.Combine(Root, "WebApp", "Areas", "Admin", "Controllers"),
            Path.Combine(Root, "WebApp", "ApiControllers")
        };

        var controllerFiles = controllerDirs
            .SelectMany(dir => Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var violations = new List<string>();
        foreach (var file in controllerFiles)
        {
            var content = File.ReadAllText(file);
            if (content.Contains("RestaurantUserDto"))
            {
                violations.Add(Path.GetRelativePath(Root, file).Replace('\\', '/'));
            }
        }

        violations.Should().BeEmpty(
            "WebApp controllers should use RestaurantUser read models and Identity commands/queries, not mutable DTO contracts.");
    }

    [Fact]
    public void WebApp_Must_Not_Call_Identity_SaveChanges()
    {
        var webAppDir = Path.Combine(Root, "WebApp");
        var csFiles = Directory.GetFiles(webAppDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            var lines = content.Split('\n');

            var identityApiNames = Regex.Matches(content, @"IIdentityModuleApi\s+([A-Za-z_][A-Za-z0-9_]*)")
                .Select(m => m.Groups[1].Value)
                .ToHashSet(StringComparer.Ordinal);
            if (identityApiNames.Count == 0) continue;

            for (var i = 0; i < lines.Length; i++)
            {
                foreach (var apiName in identityApiNames)
                {
                    if (lines[i].Contains($"{apiName}.SaveChangesAsync("))
                    {
                        violations.Add($"{Path.GetRelativePath(Root, file)}:{i + 1}");
                    }
                }
            }
        }

        violations.Should().BeEmpty("Identity module API should persist changes internally via use-case methods.");
    }

    [Fact]
    public void Identity_Module_Public_Surface_Must_Be_Intentional()
    {
        var moduleDir = Path.Combine(Root, "Modules.Identity");
        var csFiles = Directory.GetFiles(moduleDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}"));

        var allowedPrefixes = new[]
        {
            "Modules.Identity/IdentityModuleExtensions.cs",
            "Modules.Identity/Domain/"
        };

        var violations = new List<string>();
        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            if (!content.Contains("public class") && !content.Contains("public interface")) continue;

            var relativePath = Path.GetRelativePath(Root, file).Replace('\\', '/');
            if (allowedPrefixes.Any(relativePath.StartsWith))
            {
                continue;
            }

            violations.Add(relativePath);
        }

        violations.Should().BeEmpty("Identity implementation types should stay internal by default.");
    }

    [Fact]
    public void Orders_Module_Must_Not_Define_Copied_External_Domain_Entities()
    {
        var forbiddenFiles = new[]
        {
            Path.Combine(Root, "Modules.Orders", "Domain", "AppUser.cs"),
            Path.Combine(Root, "Modules.Orders", "Domain", "Restaurant.cs"),
            Path.Combine(Root, "Modules.Orders", "Domain", "Menu.cs"),
            Path.Combine(Root, "Modules.Orders", "Domain", "MenuCategory.cs"),
            Path.Combine(Root, "Modules.Orders", "Domain", "MenuItem.cs")
        };

        var existing = forbiddenFiles
            .Where(File.Exists)
            .Select(path => Path.GetRelativePath(Root, path).Replace('\\', '/'))
            .ToList();

        existing.Should().BeEmpty("Orders must not own copied user/restaurant/menu entities.");
    }

    [Fact]
    public void Orders_Migrations_Must_Not_Contain_Copied_External_Tables()
    {
        var migrationsDir = Path.Combine(Root, "Modules.Orders", "Infrastructure", "Migrations");
        var migrationFiles = Directory.GetFiles(migrationsDir, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains($"{Path.DirectorySeparatorChar}Designer.cs"))
            .ToList();

        var forbiddenMarkers = new[]
        {
            "\"AppUser\"",
            "\"Restaurant\"",
            "\"Menu\"",
            "\"MenuCategory\"",
            "\"MenuItem\""
        };

        var violations = new List<string>();
        foreach (var file in migrationFiles)
        {
            var content = File.ReadAllText(file);
            foreach (var marker in forbiddenMarkers.Where(content.Contains))
            {
                violations.Add($"{Path.GetRelativePath(Root, file).Replace('\\', '/')} -> {marker}");
            }
        }

        violations.Should().BeEmpty("Orders migrations must only define Orders-owned tables.");
    }

    [Fact]
    public void Reviews_Controllers_Must_Enforce_Restaurant_Admin_Scoping_On_Restaurant_Selection()
    {
        var controllerPaths = new[]
        {
            Path.Combine(Root, "WebApp", "Controllers", "ReviewsController.cs"),
            Path.Combine(Root, "WebApp", "Areas", "Admin", "Controllers", "ReviewsController.cs")
        };

        var requiredMarkers = new[]
        {
            "GetSelectableRestaurantsAsync",
            "IsRestaurantSelectionAllowedAsync",
            "ModelState.AddModelError(nameof(vm.RestaurantId)"
        };

        var violations = new List<string>();
        foreach (var path in controllerPaths)
        {
            File.Exists(path).Should().BeTrue($"missing expected controller file: {path}");
            var content = File.ReadAllText(path);
            foreach (var marker in requiredMarkers.Where(marker => !content.Contains(marker)))
            {
                violations.Add($"{Path.GetRelativePath(Root, path).Replace('\\', '/')} -> missing '{marker}'");
            }
        }

        violations.Should().BeEmpty(
            "Reviews Create/Edit flows must scope restaurant options for restaurant admins and reject out-of-scope selections.");
    }

    [Fact]
    public void Shared_ModuleApi_CrudStyle_Methods_Must_Be_Allowlisted()
    {
        var interfacePaths = new[]
        {
            Path.Combine(Root, "Shared.Contracts", "Restaurants", "IRestaurantsModuleApi.cs"),
            Path.Combine(Root, "Shared.Contracts", "Orders", "IOrdersModuleApi.cs"),
            Path.Combine(Root, "Shared.Contracts", "Identity", "IIdentityModuleApi.cs")
        };

        var allowlistPath = Path.Combine(
            Root,
            "WebApp.Tests",
            "Unit",
            "Architecture",
            "SharedContracts_LegacyCrud_Allowlist.txt");
        File.Exists(allowlistPath).Should().BeTrue($"allowlist file must exist: {allowlistPath}");

        var allowed = File.ReadAllLines(allowlistPath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToHashSet(StringComparer.Ordinal);

        var legacyPrefixes = new[] { "Get", "Find", "Create", "Update", "Delete", "All", "Remove" };
        var methodRegex = new Regex(@"Task(?:<[^>]+>)?\s+([A-Za-z_][A-Za-z0-9_]*)\s*\(", RegexOptions.Compiled);

        var currentLegacyMethods = new List<string>();
        foreach (var interfacePath in interfacePaths)
        {
            File.Exists(interfacePath).Should().BeTrue($"missing module API interface: {interfacePath}");
            var interfaceName = Path.GetFileNameWithoutExtension(interfacePath);
            var content = File.ReadAllText(interfacePath);

            var methods = methodRegex.Matches(content)
                .Select(m => m.Groups[1].Value)
                .Where(name => legacyPrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
                .OrderBy(name => name)
                .Select(name => $"{interfaceName}.{name}");

            currentLegacyMethods.AddRange(methods);
        }

        var newViolations = currentLegacyMethods
            .Where(method => !allowed.Contains(method))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(method => method)
            .ToList();

        newViolations.Should().BeEmpty(
            "new Shared.Contracts CRUD-style module API methods are not allowed unless explicitly added to the legacy allowlist.");
    }

    [Fact]
    public void Tests_Must_Not_Use_Legacy_App_Namespaces()
    {
        var testDir = Path.Combine(Root, "WebApp.Tests");
        var csFiles = Directory.GetFiles(testDir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}Unit{Path.DirectorySeparatorChar}BLL{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}Unit{Path.DirectorySeparatorChar}DAL{Path.DirectorySeparatorChar}") &&
                        !f.EndsWith($"{Path.DirectorySeparatorChar}Unit{Path.DirectorySeparatorChar}Architecture{Path.DirectorySeparatorChar}ModularBoundaryTests.cs"));

        var forbidden = new[] { "App.BLL", "App.DAL", "App.Domain" };
        var violations = new List<string>();

        foreach (var file in csFiles)
        {
            var content = File.ReadAllText(file);
            foreach (var marker in forbidden.Where(content.Contains))
            {
                violations.Add($"{Path.GetRelativePath(Root, file).Replace('\\', '/')} -> {marker}");
            }
        }

        violations.Should().BeEmpty();
    }

    [Fact]
    public void CustomWebApplicationFactory_Must_Use_Module_DbContexts()
    {
        var file = Path.Combine(Root, "WebApp.Tests", "CustomWebApplicationFactory.cs");
        var content = File.ReadAllText(file);

        content.Should().NotContain("AppDbContext");
        content.Should().Contain("IdentityDbContext");
        content.Should().Contain("RestaurantsDbContext");
        content.Should().Contain("OrdersDbContext");
    }

    [Fact]
    public void DataSeeder_Must_Use_Module_DbContexts()
    {
        var file = Path.Combine(Root, "WebApp.Tests", "Helpers", "DataSeeder.cs");
        var content = File.ReadAllText(file);

        content.Should().NotContain("AppDbContext");
        content.Should().Contain("IdentityDbContext");
        content.Should().Contain("RestaurantsDbContext");
        content.Should().Contain("OrdersDbContext");
    }

    [Fact]
    public void WebAppTestsCsproj_Must_Not_Disable_Modular_Test_Infrastructure()
    {
        var file = Path.Combine(Root, "WebApp.Tests", "WebApp.Tests.csproj");
        var content = File.ReadAllText(file);

        content.Should().NotContain("Compile Remove=\"CustomWebApplicationFactory.cs\"");
        content.Should().NotContain("Compile Remove=\"Helpers/DataSeeder.cs\"");
    }

    [Fact]
    public void Module_Test_Folders_Must_Not_Cross_Module_Infrastructure()
    {
        var checks = new[]
        {
            ("WebApp.Tests/Integration/Restaurants", "Modules.Orders.Infrastructure"),
            ("WebApp.Tests/Integration/Restaurants", "Modules.Identity.Infrastructure"),
            ("WebApp.Tests/Integration/Orders", "Modules.Restaurants.Infrastructure"),
            ("WebApp.Tests/Integration/Orders", "Modules.Identity.Infrastructure"),
            ("WebApp.Tests/Integration/Identity", "Modules.Restaurants.Infrastructure"),
            ("WebApp.Tests/Integration/Identity", "Modules.Orders.Infrastructure")
        };

        var violations = new List<string>();
        foreach (var (folder, forbiddenNs) in checks)
        {
            var abs = Path.Combine(Root, folder.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(abs)) continue;

            var csFiles = Directory.GetFiles(abs, "*.cs", SearchOption.AllDirectories);
            foreach (var file in csFiles)
            {
                var content = File.ReadAllText(file);
                if (content.Contains(forbiddenNs))
                {
                    violations.Add($"{Path.GetRelativePath(Root, file).Replace('\\', '/')} -> {forbiddenNs}");
                }
            }
        }

        violations.Should().BeEmpty();
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Project_modular.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate repository root.");
    }

    private static IEnumerable<string> GetSourceAndRazorFiles(string root)
    {
        return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.GetFiles(root, "*.cshtml", SearchOption.AllDirectories))
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") &&
                        !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"));
    }
}
