namespace Shared.Contracts.Restaurants;

public class ReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Summary { get; set; }
    public ReviewUserDto? AppUser { get; set; }
    public ReviewRestaurantDto? Restaurant { get; set; }
}

public class ReviewUserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
}

public class ReviewRestaurantDto
{
    public Guid Id { get; set; }
    public string? RestaurantName { get; set; }
}
