using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateRestaurantCommandHandler : IRequestHandler<CreateRestaurantCommand, RestaurantDto>
{
    private readonly IRestaurantService _restaurants;
    private readonly IRestaurantsUow _uow;

    public CreateRestaurantCommandHandler(IRestaurantService restaurants, IRestaurantsUow uow)
    {
        _restaurants = restaurants;
        _uow = uow;
    }

    public async Task<RestaurantDto> Handle(CreateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var entity = new RestaurantDto
        {
            Id = Guid.NewGuid(),
            RestaurantName = request.RestaurantName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description,
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary
        };

        _restaurants.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateRestaurantCommandHandler : IRequestHandler<UpdateRestaurantCommand, RestaurantDto?>
{
    private readonly IRestaurantService _restaurants;
    private readonly IRestaurantsUow _uow;

    public UpdateRestaurantCommandHandler(IRestaurantService restaurants, IRestaurantsUow uow)
    {
        _restaurants = restaurants;
        _uow = uow;
    }

    public async Task<RestaurantDto?> Handle(UpdateRestaurantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _restaurants.FindAsync(request.RestaurantId, request.AppUserId);
        if (entity == null) return null;

        entity.RestaurantName = request.RestaurantName;
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description;
        entity.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone;
        entity.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email;
        entity.Summary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary;

        var updated = await _restaurants.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteRestaurantCommandHandler : IRequestHandler<DeleteRestaurantCommand, bool>
{
    private readonly IRestaurantService _restaurants;
    private readonly IRestaurantsUow _uow;

    public DeleteRestaurantCommandHandler(IRestaurantService restaurants, IRestaurantsUow uow)
    {
        _restaurants = restaurants;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteRestaurantCommand request, CancellationToken cancellationToken)
    {
        var entity = await _restaurants.FindAsync(request.RestaurantId, request.AppUserId);
        if (entity == null) return false;

        await _restaurants.RemoveAsync(request.RestaurantId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
