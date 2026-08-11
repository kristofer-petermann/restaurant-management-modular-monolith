using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record CreateOpeningHourCommand(
    Guid RestaurantId,
    string? DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    string? Summary,
    Guid AppUserId = default) : IRequest<OpeningHourDto>;
