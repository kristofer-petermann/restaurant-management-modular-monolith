namespace Shared.Contracts.Restaurants;

public class AddressDto
{
    public Guid Id { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Summary { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RestaurantId { get; set; }
    public AddressUserDto? User { get; set; }
    public AddressRestaurantDto? Restaurant { get; set; }
}

public class AddressUserDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
}

public class AddressRestaurantDto
{
    public Guid Id { get; set; }
    public string? RestaurantName { get; set; }
}
