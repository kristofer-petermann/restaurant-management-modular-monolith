using System.ComponentModel.DataAnnotations;

namespace WebApp.ApiContracts.v1;

public class ReviewUpdateRequest
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RestaurantId { get; set; }
    [Range(1, 10)]
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Summary { get; set; }
}
