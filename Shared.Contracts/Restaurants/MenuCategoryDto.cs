namespace Shared.Contracts.Restaurants;

public class MenuCategoryDto
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string? Name { get; set; }
    public string? Summary { get; set; }
    public MenuCategoryMenuDto? Menu { get; set; }
}

public class MenuCategoryMenuDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}
