using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, AddressDto>
{
    private readonly IAddressService _addresses;
    private readonly IRestaurantsUow _uow;

    public CreateAddressCommandHandler(IAddressService addresses, IRestaurantsUow uow)
    {
        _addresses = addresses;
        _uow = uow;
    }

    public async Task<AddressDto> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = new AddressDto
        {
            Id = Guid.NewGuid(),
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? string.Empty : request.Summary,
            UserId = request.UserId,
            RestaurantId = request.RestaurantId
        };

        _addresses.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, AddressDto?>
{
    private readonly IAddressService _addresses;
    private readonly IRestaurantsUow _uow;

    public UpdateAddressCommandHandler(IAddressService addresses, IRestaurantsUow uow)
    {
        _addresses = addresses;
        _uow = uow;
    }

    public async Task<AddressDto?> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = await _addresses.FindAsync(request.AddressId, request.AppUserId);
        if (entity == null) return null;

        entity.Street = request.Street;
        entity.City = request.City;
        entity.PostalCode = request.PostalCode;
        entity.Country = request.Country;
        entity.Summary = request.Summary;
        entity.UserId = request.UserId;
        entity.RestaurantId = request.RestaurantId;

        var updated = await _addresses.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, bool>
{
    private readonly IAddressService _addresses;
    private readonly IRestaurantsUow _uow;

    public DeleteAddressCommandHandler(IAddressService addresses, IRestaurantsUow uow)
    {
        _addresses = addresses;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var entity = await _addresses.FindAsync(request.AddressId, request.AppUserId);
        if (entity == null) return false;

        await _addresses.RemoveAsync(request.AddressId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
