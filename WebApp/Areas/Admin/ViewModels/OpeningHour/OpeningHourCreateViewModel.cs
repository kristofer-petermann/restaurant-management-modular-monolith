using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.Areas.Admin.ViewModels.OpeningHour;

public class OpeningHourCreateViewModel
{
    public Guid RestaurantId { get; set; }
    public string DayOfWeek { get; set; } = default!;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}

