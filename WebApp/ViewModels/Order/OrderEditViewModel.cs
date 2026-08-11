using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.Order;

public class OrderEditViewModel
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string OrderStatus { get; set; } = default!;
    public decimal TotalPrice { get; set; }
    public DateTime PickupTime { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}

