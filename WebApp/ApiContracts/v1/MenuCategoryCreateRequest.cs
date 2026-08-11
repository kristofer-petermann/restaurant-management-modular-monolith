namespace WebApp.ApiContracts.v1;

public class MenuCategoryCreateRequest
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = default!;
    public string? Summary { get; set; }
}

