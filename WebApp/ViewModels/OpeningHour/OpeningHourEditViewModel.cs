using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApp.ViewModels.OpeningHour;

public class OpeningHourEditViewModel
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string DayOfWeek { get; set; } = default!;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
    public IEnumerable<SelectListItem> RestaurantOptions { get; set; } = new List<SelectListItem>();
}

