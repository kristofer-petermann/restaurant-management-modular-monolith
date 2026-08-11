using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Base.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Contracts.Identity;
using Modules.Identity.Domain;
using Modules.Identity.Infrastructure;

namespace Modules.Identity.Application.Services;

internal class IdentityAccountService : IIdentityAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IdentityDbContext _context;
    private readonly ILogger<IdentityAccountService> _logger;

    public IdentityAccountService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration,
        IdentityDbContext context,
        ILogger<IdentityAccountService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResultDto<AuthTokenResponseDto>> RegisterAsync(AuthRegisterRequestDto request, int expiresInSeconds)
    {
        expiresInSeconds = ResolveExpires(expiresInSeconds);

        var appUser = await _userManager.FindByEmailAsync(request.Email);
        if (appUser != null)
        {
            _logger.LogWarning("User with email {} is already registered", request.Email);
            return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, $"User with email {request.Email} is already registered");
        }

        var refreshToken = new AppRefreshToken();
        appUser = new AppUser
        {
            Email = request.Email,
            UserName = request.Email,
            RefreshTokens = new List<AppRefreshToken> { refreshToken }
        };
        refreshToken.AppUser = appUser;

        var result = await _userManager.CreateAsync(appUser, request.Password);
        if (!result.Succeeded)
        {
            return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, result.Errors.First().Description);
        }

        appUser = await _userManager.FindByEmailAsync(appUser.Email!);
        if (appUser == null)
        {
            _logger.LogWarning("User with email {} is not found after registration", request.Email);
            return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, $"User with email {request.Email} is not found after registration");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var jwt = GenerateJwt(claimsPrincipal.Claims, expiresInSeconds);

        return Success(new AuthTokenResponseDto
        {
            Jwt = jwt,
            RefreshToken = refreshToken.RefreshToken
        });
    }

    public async Task<AuthResultDto<AuthTokenResponseDto>> LoginAsync(AuthLoginRequestDto request, int expiresInSeconds)
    {
        expiresInSeconds = ResolveExpires(expiresInSeconds);

        var appUser = await _userManager.FindByEmailAsync(request.Email);
        if (appUser == null)
        {
            _logger.LogWarning("WebApi login failed, email {} not found", request.Email);
            return Fail<AuthTokenResponseDto>(HttpStatusCode.NotFound, "User/Password problem");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(appUser, request.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning("WebApi login failed, password for email {} was wrong", request.Email);
            return Fail<AuthTokenResponseDto>(HttpStatusCode.NotFound, "User/Password problem");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);

        if (!_context.Database.ProviderName!.Contains("InMemory"))
        {
            var deletedRows = await _context.RefreshTokens
                .Where(t => t.AppUserId == appUser.Id && t.ExpirationDT < DateTime.UtcNow)
                .ExecuteDeleteAsync();
            _logger.LogInformation("Deleted {} refresh tokens", deletedRows);
        }

        var refreshToken = new AppRefreshToken { AppUserId = appUser.Id };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Success(new AuthTokenResponseDto
        {
            Jwt = GenerateJwt(claimsPrincipal.Claims, expiresInSeconds),
            RefreshToken = refreshToken.RefreshToken
        });
    }

    public async Task<AuthResultDto<AuthTokenResponseDto>> RefreshTokenDataAsync(AuthRefreshRequestDto request, int expiresInSeconds)
    {
        expiresInSeconds = ResolveExpires(expiresInSeconds);

        JwtSecurityToken? jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(request.Jwt);
            if (jwt == null) return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, "No token");
        }
        catch
        {
            return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, "No token");
        }

        if (!IdentityHelpers.ValidateJWT(request.Jwt, JwtKey(), JwtIssuer(), JwtAudience()))
        {
            return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, "JWT validation fail");
        }

        var userEmail = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        if (userEmail == null) return Fail<AuthTokenResponseDto>(HttpStatusCode.BadRequest, "No email in jwt");

        var appUser = await _userManager.FindByEmailAsync(userEmail);
        if (appUser == null) return Fail<AuthTokenResponseDto>(HttpStatusCode.NotFound, $"User with email {userEmail} not found");

        await _context.Entry(appUser).Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x =>
                (x.RefreshToken == request.RefreshToken && x.ExpirationDT > DateTime.UtcNow) ||
                (x.PreviousRefreshToken == request.RefreshToken && x.PreviousExpirationDT > DateTime.UtcNow))
            .ToListAsync();

        if (appUser.RefreshTokens == null || appUser.RefreshTokens.Count == 0)
        {
            return Fail<AuthTokenResponseDto>(HttpStatusCode.NotFound,
                $"RefreshTokens collection is null or empty - {appUser.RefreshTokens?.Count}");
        }

        if (appUser.RefreshTokens.Count != 1)
        {
            return Fail<AuthTokenResponseDto>(HttpStatusCode.NotFound, "More than one valid refresh token found");
        }

        var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(appUser);
        var refreshToken = appUser.RefreshTokens.First();

        if (refreshToken.RefreshToken == request.RefreshToken)
        {
            refreshToken.PreviousRefreshToken = refreshToken.RefreshToken;
            refreshToken.PreviousExpirationDT = DateTime.UtcNow.AddMinutes(1);
            refreshToken.RefreshToken = Guid.NewGuid().ToString();
            refreshToken.ExpirationDT = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();
        }

        return Success(new AuthTokenResponseDto
        {
            Jwt = GenerateJwt(claimsPrincipal.Claims, expiresInSeconds),
            RefreshToken = refreshToken.RefreshToken
        });
    }

    public async Task<AuthResultDto<int>> LogoutAsync(Guid userId, string refreshToken)
    {
        var appUser = await _context.Users.Where(u => u.Id == userId).SingleOrDefaultAsync();
        if (appUser == null)
        {
            return Fail<int>(HttpStatusCode.NotFound, "User/Password problem");
        }

        await _context.Entry(appUser)
            .Collection(u => u.RefreshTokens!)
            .Query()
            .Where(x => x.RefreshToken == refreshToken || x.PreviousRefreshToken == refreshToken)
            .ToListAsync();

        foreach (var appRefreshToken in appUser.RefreshTokens!)
        {
            _context.RefreshTokens.Remove(appRefreshToken);
        }

        var deleteCount = await _context.SaveChangesAsync();
        return Success(deleteCount);
    }

    private int ResolveExpires(int expiresInSeconds)
    {
        if (expiresInSeconds <= 0) expiresInSeconds = int.MaxValue;
        var configured = _configuration.GetValue<int>("JWT:ExpiresInSeconds");
        return expiresInSeconds < configured ? expiresInSeconds : configured;
    }

    private string GenerateJwt(IEnumerable<Claim> claims, int expiresInSeconds)
    {
        return IdentityHelpers.GenerateJwt(claims, JwtKey(), JwtIssuer(), JwtAudience(), expiresInSeconds);
    }

    private string JwtKey() => _configuration.GetValue<string>("JWT:Key")!;
    private string JwtIssuer() => _configuration.GetValue<string>("JWT:Issuer")!;
    private string JwtAudience() => _configuration.GetValue<string>("JWT:Audience")!;

    private static AuthResultDto<T> Success<T>(T data) => new()
    {
        Succeeded = true,
        StatusCode = (int)HttpStatusCode.OK,
        Data = data
    };

    private static AuthResultDto<T> Fail<T>(HttpStatusCode statusCode, string error) => new()
    {
        Succeeded = false,
        StatusCode = (int)statusCode,
        Error = error
    };
}
