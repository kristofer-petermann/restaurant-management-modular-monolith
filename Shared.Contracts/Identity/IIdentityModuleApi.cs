namespace Shared.Contracts.Identity;

public interface IIdentityModuleApi
{
    Task<AuthResultDto<AuthTokenResponseDto>> RegisterAsync(AuthRegisterRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<AuthTokenResponseDto>> LoginAsync(AuthLoginRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<AuthTokenResponseDto>> RefreshTokenDataAsync(AuthRefreshRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<int>> LogoutAsync(Guid userId, string refreshToken);

    Task<bool> IsRestaurantAdminAsync(Guid userId, Guid? restaurantId = null);
    Task<List<Guid>> GetAdminRestaurantIdsAsync(Guid userId);
    Task<List<AllowedRestaurantDto>> GetAllowedRestaurantsAsync(Guid userId);
    Task<List<UserOptionDto>> GetUsersAsync();
}
