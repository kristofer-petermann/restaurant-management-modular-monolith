namespace WebApp.ApiContracts.v1;

public class OpeningHourUpdateRequest
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string DayOfWeek { get; set; } = default!;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
}

