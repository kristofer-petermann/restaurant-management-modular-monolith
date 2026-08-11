namespace WebApp.ApiContracts.v1;

public class OpeningHourCreateRequest
{
    public Guid RestaurantId { get; set; }
    public string DayOfWeek { get; set; } = default!;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
}

