using Microsoft.AspNetCore.Mvc.Rendering;
using Shared.Contracts.Identity;

namespace WebApp.ViewModels.RestaurantUser;

public class RestaurantUserCreateViewModel
{
    public Guid RestaurantId { get; set; }
    public Guid UserId { get; set; }
    public RestaurantUserRole UserRole { get; set; }
    public string? Role { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> UserOptions { get; set; } = new List<SelectListItem>();
}
