using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.Menu;

public class MenuEditViewModel
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}

