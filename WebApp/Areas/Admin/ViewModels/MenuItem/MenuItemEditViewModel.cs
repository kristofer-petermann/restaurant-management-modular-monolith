using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.MenuItem;

public class MenuItemEditViewModel
{
    public Guid Id { get; set; }
    public Guid MenuCategoryId { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> MenuCategoryOptions { get; set; } = new List<SelectListItem>();
}

