using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Contracts.Identity;

namespace WebApp.Areas.Admin.ViewModels.RestaurantUser;

public class RestaurantUserCreateViewModel
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public RestaurantUserRole UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }

    [ValidateNever]
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();

    [ValidateNever]
    public IEnumerable<SelectListItem> UserOptions { get; set; } = new List<SelectListItem>();
}
