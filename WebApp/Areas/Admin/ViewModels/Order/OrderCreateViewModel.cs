using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Order;

public class OrderCreateViewModel
{
    public Guid RestaurantId { get; set; }
    public string OrderStatus { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public DateTime PickupTime { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}
