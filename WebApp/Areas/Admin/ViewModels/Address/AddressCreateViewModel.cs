using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.Address;

public class AddressCreateViewModel
{
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Summary { get; set; }

    public Guid? RestaurantId { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
    public Guid? UserId { get; set; }
    public IEnumerable<SelectListItem> UserOptions { get; set; } = new List<SelectListItem>();
}
