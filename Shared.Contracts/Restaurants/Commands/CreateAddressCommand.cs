using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record CreateAddressCommand(
    string? Street,
    string? City,
    string? PostalCode,
    string? Country,
    string? Summary,
    Guid? UserId,
    Guid? RestaurantId,
    Guid AppUserId = default) : IRequest<AddressDto>;
