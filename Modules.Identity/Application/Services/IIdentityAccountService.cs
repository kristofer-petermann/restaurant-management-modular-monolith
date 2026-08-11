using Shared.Contracts.Identity;

namespace Modules.Identity.Application.Services;

internal interface IIdentityAccountService
{
    Task<AuthResultDto<AuthTokenResponseDto>> RegisterAsync(AuthRegisterRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<AuthTokenResponseDto>> LoginAsync(AuthLoginRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<AuthTokenResponseDto>> RefreshTokenDataAsync(AuthRefreshRequestDto request, int expiresInSeconds);
    Task<AuthResultDto<int>> LogoutAsync(Guid userId, string refreshToken);
}
