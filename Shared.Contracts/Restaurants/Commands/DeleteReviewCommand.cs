using MediatR;

namespace Shared.Contracts.Restaurants.Commands;

public sealed record DeleteReviewCommand(Guid ReviewId, Guid AppUserId = default) : IRequest<bool>;
