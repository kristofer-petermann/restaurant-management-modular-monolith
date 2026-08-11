using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteAddressCommand(Guid AddressId, Guid AppUserId = default) : IRequest<bool>;
