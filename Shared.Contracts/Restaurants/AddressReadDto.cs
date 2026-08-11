namespace Shared.Contracts.Restaurants;

public class AddressReadDto
{
    public Guid Id { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public string? UserEmail { get; set; }
}
