using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.OrderRow;

public class OrderRowCreateViewModel
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> OrderOptions { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> MenuItemOptions { get; set; } = new List<SelectListItem>();
}

