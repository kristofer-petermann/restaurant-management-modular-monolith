namespace Shared.Contracts.Restaurants;

public class OpeningHourReadDto
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string? Summary { get; set; }
    public string? RestaurantName { get; set; }
}
