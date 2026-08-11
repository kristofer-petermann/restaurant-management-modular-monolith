using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Modules.Identity;
using Modules.Orders;
using Modules.Restaurants;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApp;
using Base.Domain;

var builder = WebApplication.CreateBuilder(args);

LangStr.DefaultCulture = builder.Configuration["LangStrDefaultCulture"] ?? "en";

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(IdentityModuleExtensions).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(RestaurantsModuleExtensions).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OrdersModuleExtensions).Assembly);
});
builder.Services.AddIdentityModule(connectionString);
builder.Services.AddRestaurantsModule(connectionString);
builder.Services.AddOrdersModule(connectionString);

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
builder.Services
    .AddAuthentication()
    .AddCookie(options => { options.SlidingExpiration = true; })
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false; // TODO: set to true in production!
        cfg.SaveToken = true;
        cfg.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]!)),
            ClockSkew = TimeSpan.Zero // remove delay of token when expire
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RootOnly", policy => policy.RequireRole("root"));
});

var supportedCultures = builder.Configuration
    .GetSection("SupportedCultures")
    .GetChildren()
    .Select(x => new CultureInfo(x.Value!))
    .ToArray();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // datetime and currency support
    options.SupportedCultures = supportedCultures;
    // UI translated strings
    options.SupportedUICultures = supportedCultures;
    // if nothing is found, use this
    options.DefaultRequestCulture = new RequestCulture("et-EE", "et-EE");
    options.SetDefaultCulture("et-EE");

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        // Order is important, it's in which order they will be evaluated
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsAllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-Version", "X-Version-Created-At");
    });
});


var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    // in case of no explicit version
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

apiVersioningBuilder.AddApiExplorer(options =>
{
    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
    // note: the specified format code will format the version as "'v'major[.minor][-status]"
    options.GroupNameFormat = "'v'VVV";

    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
    // can also be used to control the format of the API version in route templates
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardLimit = null; // null = no limit (0 means don't process any!)
    options.ForwardedHeaders = ForwardedHeaders.All;
    // Trust all IP addresses - safe when behind a known reverse proxy
    options.KnownProxies.Clear();
    options.KnownIPNetworks.Clear();
    options.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.Any, 0));
    options.KnownIPNetworks.Add(new System.Net.IPNetwork(IPAddress.IPv6Any, 0));
});

var app = builder.Build();
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
await SetupAppDataAsync(app, app.Configuration, connectionString);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization(options: app.Services
    .GetService<IOptions<RequestLocalizationOptions>>()!.Value);

app.UseCors("CorsAllowAll");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant()
        );
    }
    // serve from root
    // options.RoutePrefix = string.Empty;
});

app.MapStaticAssets();

app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

static async Task SetupAppDataAsync(WebApplication app, IConfiguration configuration, string connectionString)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("WebApp.DataSetup");

    var dropDatabase = configuration.GetValue<bool>("DataInitialization:DropDatabase");
    var migrateDatabase = configuration.GetValue<bool>("DataInitialization:MigrateDatabase");
    var seedIdentity = configuration.GetValue<bool>("DataInitialization:SeedIdentity");
    var seedData = configuration.GetValue<bool>("DataInitialization:SeedData");

    if (dropDatabase || migrateDatabase)
    {
        IdentityModuleExtensions.WaitDbConnection(connectionString, logger);

        if (dropDatabase)
        {
            logger.LogWarning("Dropping shared module database");
            await app.Services.DropDatabaseAsync();
        }

        logger.LogInformation("Migrating Identity module");
        await app.Services.MigrateIdentityModuleAsync();

        logger.LogInformation("Migrating Restaurants module");
        await app.Services.MigrateRestaurantsModuleAsync();

        logger.LogInformation("Migrating Orders module");
        await app.Services.MigrateOrdersModuleAsync();
    }

    if (seedIdentity)
    {
        logger.LogInformation("Seeding Identity module");
        await app.Services.SeedIdentityModuleAsync();
    }

    if (seedData)
    {
        logger.LogInformation("Seeding Restaurants module");
        await app.Services.SeedRestaurantsModuleAsync();

        logger.LogInformation("Seeding Orders module");
        await app.Services.SeedOrdersModuleAsync();
    }
}

app.Run();

public partial class Program
{
}
