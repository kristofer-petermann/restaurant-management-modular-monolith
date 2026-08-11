using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record UpdateOpeningHourCommand(
    Guid OpeningHourId,
    Guid RestaurantId,
    string? DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    string? Summary,
    Guid AppUserId = default) : IRequest<OpeningHourDto?>;
