using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Menu;

public class MenuCreateViewModel
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
    public string? Summary { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}

