namespace WebApp.ApiContracts.v1;

public class MenuCategoryUpdateRequest
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string Name { get; set; } = default!;
    public string? Summary { get; set; }
}

