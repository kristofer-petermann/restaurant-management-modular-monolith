using System.Net;
using Asp.Versioning;
using Base.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Identity;
using WebApp.ApiContracts.v1;
using WebApp.ApiContracts.v1.Identity;

namespace WebApp.ApiControllers.Identity;

[ApiVersion("1.0")]
[ApiController]
[Route("/api/v{version:apiVersion}/identity/[controller]/[action]")]
public class AccountController : ControllerBase
{
    private readonly IIdentityModuleApi _identity;

    public AccountController(IIdentityModuleApi identity)
    {
        _identity = identity;
    }

    [HttpPost]
    [Produces("application/json")]
    [Consumes("application/json")]
    [ProducesResponseType<JWTResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<RestApiErrorResponse>((int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<JWTResponse>> Register([FromBody] RegisterInfo registrationData, [FromQuery] int expiresInSeconds)
    {
        var result = await _identity.RegisterAsync(new AuthRegisterRequestDto
        {
            Email = registrationData.Email,
            Password = registrationData.Password
        }, expiresInSeconds);

        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<JWTResponse>> Login([FromBody] LoginInfo loginInfo, [FromQuery] int expiresInSeconds)
    {
        var result = await _identity.LoginAsync(new AuthLoginRequestDto
        {
            Email = loginInfo.Email,
            Password = loginInfo.Password
        }, expiresInSeconds);

        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<JWTResponse>> RefreshTokenData([FromBody] TokenRefreshInfo tokenRefreshInfo, [FromQuery] int expiresInSeconds)
    {
        var result = await _identity.RefreshTokenDataAsync(new AuthRefreshRequestDto
        {
            Jwt = tokenRefreshInfo.Jwt,
            RefreshToken = tokenRefreshInfo.RefreshToken
        }, expiresInSeconds);

        return ToActionResult(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost]
    public async Task<ActionResult> Logout([FromBody] LogoutInfo logout)
    {
        Guid userId;
        try
        {
            userId = User.GetUserId();
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new RestApiErrorResponse
            {
                Status = HttpStatusCode.BadRequest,
                Error = "Invalid refresh token"
            });
        }

        var result = await _identity.LogoutAsync(userId, logout.RefreshToken);
        if (result.Succeeded)
        {
            return Ok(new { TokenDeleteCount = result.Data });
        }

        if (result.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return NotFound(new RestApiErrorResponse
            {
                Status = HttpStatusCode.NotFound,
                Error = result.Error ?? "User/Password problem"
            });
        }

        return BadRequest(new RestApiErrorResponse
        {
            Status = HttpStatusCode.BadRequest,
            Error = result.Error ?? "Invalid refresh token"
        });
    }

    private ActionResult<JWTResponse> ToActionResult(AuthResultDto<AuthTokenResponseDto> result)
    {
        if (result.Succeeded && result.Data != null)
        {
            return Ok(new JWTResponse
            {
                Jwt = result.Data.Jwt,
                RefreshToken = result.Data.RefreshToken
            });
        }

        if (result.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return NotFound(result.Error);
        }

        return BadRequest(new RestApiErrorResponse
        {
            Status = HttpStatusCode.BadRequest,
            Error = result.Error ?? "Request failed"
        });
    }
}
