using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.Address;

public class AddressEditViewModel
{
    public Guid Id { get; set; }
    
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Summary { get; set; }
    
    public Guid? RestaurantId { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}