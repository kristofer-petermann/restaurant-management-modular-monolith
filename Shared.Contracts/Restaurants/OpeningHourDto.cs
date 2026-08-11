namespace Shared.Contracts.Restaurants;

public class OpeningHourDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string? DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
    public OpeningHourRestaurantDto? Restaurant { get; set; }
}

public class OpeningHourRestaurantDto
{
    public Guid Id { get; set; }
    public string? RestaurantName { get; set; }
}
