using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.Payment;

public class PaymentCreateViewModel
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = default!;
    public string PaymentMethod { get; set; } = default!;
    public DateTime? PaidAt { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> OrderOptions { get; set; } = new List<SelectListItem>();
}

