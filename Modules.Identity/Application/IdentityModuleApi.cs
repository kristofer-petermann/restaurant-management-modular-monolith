using Modules.Identity.Application.Services;
using Shared.Contracts.Identity;

namespace Modules.Identity.Application;

internal class IdentityModuleApi : IIdentityModuleApi
{
    private readonly IIdentityAccountService _accountService;
    private readonly IRestaurantUserService _restaurantUsers;

    public IdentityModuleApi(
        IIdentityAccountService accountService,
        Modules.Identity.Application.Services.IRestaurantUserService restaurantUsers)
    {
        _accountService = accountService;
        _restaurantUsers = restaurantUsers;
    }

    public Task<AuthResultDto<AuthTokenResponseDto>> RegisterAsync(AuthRegisterRequestDto request, int expiresInSeconds) =>
        _accountService.RegisterAsync(request, expiresInSeconds);

    public Task<AuthResultDto<AuthTokenResponseDto>> LoginAsync(AuthLoginRequestDto request, int expiresInSeconds) =>
        _accountService.LoginAsync(request, expiresInSeconds);

    public Task<AuthResultDto<AuthTokenResponseDto>> RefreshTokenDataAsync(AuthRefreshRequestDto request, int expiresInSeconds) =>
        _accountService.RefreshTokenDataAsync(request, expiresInSeconds);

    public Task<AuthResultDto<int>> LogoutAsync(Guid userId, string refreshToken) =>
        _accountService.LogoutAsync(userId, refreshToken);

    public Task<bool> IsRestaurantAdminAsync(Guid userId, Guid? restaurantId = null) => _restaurantUsers.IsRestaurantAdminAsync(userId, restaurantId);
    public Task<List<Guid>> GetAdminRestaurantIdsAsync(Guid userId) => _restaurantUsers.GetAdminRestaurantIdsAsync(userId);
    public Task<List<AllowedRestaurantDto>> GetAllowedRestaurantsAsync(Guid userId) => _restaurantUsers.GetAllowedRestaurantsAsync(userId);
    public Task<List<UserOptionDto>> GetUsersAsync() => _restaurantUsers.GetUsersAsync();
}
