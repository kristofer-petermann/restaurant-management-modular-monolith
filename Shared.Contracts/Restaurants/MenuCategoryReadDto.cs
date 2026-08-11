namespace Shared.Contracts.Restaurants;

public class MenuCategoryReadDto
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? MenuName { get; set; }
}
