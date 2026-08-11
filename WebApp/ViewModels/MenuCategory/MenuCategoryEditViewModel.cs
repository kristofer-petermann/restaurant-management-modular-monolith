using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.MenuCategory;

public class MenuCategoryEditViewModel
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string Name { get; set; } = default!;
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> MenuOptions { get; set; } = new List<SelectListItem>();
}

