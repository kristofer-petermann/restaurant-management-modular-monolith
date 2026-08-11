namespace Shared.Contracts.Identity;

public class AuthRegisterRequestDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AuthLoginRequestDto
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class AuthRefreshRequestDto
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class AuthTokenResponseDto
{
    public string Jwt { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class AuthResultDto<T>
{
    public bool Succeeded { get; set; }
    public int StatusCode { get; set; }
    public string? Error { get; set; }
    public T? Data { get; set; }
}
