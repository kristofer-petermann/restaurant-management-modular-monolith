namespace WebApp.ApiContracts.v1;

public class AddressResponse
{
    public Guid Id { get; set; }
    public string Street { get; set; } = default!;
    public string City { get; set; } = default!;
    public string PostalCode { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Summary { get; set; }
    public Guid? UserId { get; set; }
    public Guid? RestaurantId { get; set; }
}

