using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace WebApp.ViewModels.Review;

public class ReviewCreateViewModel
{
    public Guid RestaurantId { get; set; }
    [Range(1, 10)]
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}
