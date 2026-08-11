using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class CreateOpeningHourCommandHandler : IRequestHandler<CreateOpeningHourCommand, OpeningHourDto>
{
    private readonly IOpeningHourService _openingHours;
    private readonly IRestaurantsUow _uow;

    public CreateOpeningHourCommandHandler(IOpeningHourService openingHours, IRestaurantsUow uow)
    {
        _openingHours = openingHours;
        _uow = uow;
    }

    public async Task<OpeningHourDto> Handle(CreateOpeningHourCommand request, CancellationToken cancellationToken)
    {
        var entity = new OpeningHourDto
        {
            Id = Guid.NewGuid(),
            RestaurantId = request.RestaurantId,
            DayOfWeek = request.DayOfWeek,
            OpenTime = request.OpenTime,
            CloseTime = request.CloseTime,
            Summary = string.IsNullOrWhiteSpace(request.Summary) ? string.Empty : request.Summary
        };

        _openingHours.Add(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return entity;
    }
}

internal sealed class UpdateOpeningHourCommandHandler : IRequestHandler<UpdateOpeningHourCommand, OpeningHourDto?>
{
    private readonly IOpeningHourService _openingHours;
    private readonly IRestaurantsUow _uow;

    public UpdateOpeningHourCommandHandler(IOpeningHourService openingHours, IRestaurantsUow uow)
    {
        _openingHours = openingHours;
        _uow = uow;
    }

    public async Task<OpeningHourDto?> Handle(UpdateOpeningHourCommand request, CancellationToken cancellationToken)
    {
        var entity = await _openingHours.FindAsync(request.OpeningHourId, request.AppUserId);
        if (entity == null) return null;

        entity.RestaurantId = request.RestaurantId;
        entity.DayOfWeek = request.DayOfWeek;
        entity.OpenTime = request.OpenTime;
        entity.CloseTime = request.CloseTime;
        entity.Summary = request.Summary;

        var updated = await _openingHours.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteOpeningHourCommandHandler : IRequestHandler<DeleteOpeningHourCommand, bool>
{
    private readonly IOpeningHourService _openingHours;
    private readonly IRestaurantsUow _uow;

    public DeleteOpeningHourCommandHandler(IOpeningHourService openingHours, IRestaurantsUow uow)
    {
        _openingHours = openingHours;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteOpeningHourCommand request, CancellationToken cancellationToken)
    {
        var entity = await _openingHours.FindAsync(request.OpeningHourId, request.AppUserId);
        if (entity == null) return false;

        await _openingHours.RemoveAsync(request.OpeningHourId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
