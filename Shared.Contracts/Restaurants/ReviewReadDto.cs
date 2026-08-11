namespace Shared.Contracts.Restaurants;

public class ReviewReadDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Summary { get; set; }
    public string? RestaurantName { get; set; }
    public string? UserName { get; set; }
}
