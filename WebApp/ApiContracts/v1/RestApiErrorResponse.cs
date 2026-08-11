namespace WebApp.ApiContracts.v1;

public class RestApiErrorResponse
{
    public System.Net.HttpStatusCode Status { get; set; }
    public string Error { get; set; } = default!;
}