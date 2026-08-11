using MediatR;
using Modules.Restaurants.Application.Services;
using Modules.Restaurants.Infrastructure;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;

namespace Modules.Restaurants.Application.Handlers;

internal sealed class UpdateReviewCommandHandler : IRequestHandler<UpdateReviewCommand, ReviewDto?>
{
    private readonly IReviewService _reviews;
    private readonly IRestaurantsUow _uow;

    public UpdateReviewCommandHandler(IReviewService reviews, IRestaurantsUow uow)
    {
        _reviews = reviews;
        _uow = uow;
    }

    public async Task<ReviewDto?> Handle(UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating is < 1 or > 10)
        {
            throw new ArgumentException("Rating must be between 1 and 10.", nameof(request));
        }

        var entity = await _reviews.FindAsync(request.ReviewId, request.AppUserId);
        if (entity == null) return null;

        entity.RestaurantId = request.RestaurantId;
        entity.UserId = request.UserId;
        entity.Rating = request.Rating;
        entity.Comment = string.IsNullOrWhiteSpace(request.Comment) ? string.Empty : request.Comment;
        entity.CreatedAt = request.CreatedAt;
        entity.Summary = request.Summary;

        var updated = await _reviews.UpdateAsync(entity, request.AppUserId);
        await _uow.SaveChangesAsync();
        return updated;
    }
}

internal sealed class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, bool>
{
    private readonly IReviewService _reviews;
    private readonly IRestaurantsUow _uow;

    public DeleteReviewCommandHandler(IReviewService reviews, IRestaurantsUow uow)
    {
        _reviews = reviews;
        _uow = uow;
    }

    public async Task<bool> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var entity = await _reviews.FindAsync(request.ReviewId, request.AppUserId);
        if (entity == null) return false;

        await _reviews.RemoveAsync(request.ReviewId, request.AppUserId);
        await _uow.SaveChangesAsync();
        return true;
    }
}
