using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IReviewRepository
{
    Task<List<Review>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Review?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<Review?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Review?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(Review entity);
    void Remove(Review entity);
}
