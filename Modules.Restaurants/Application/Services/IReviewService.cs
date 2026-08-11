using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IReviewService
{
    Task<IEnumerable<ReviewReadDto>> AllReadAsync(Guid appUserId = default);
    Task<ReviewReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<ReviewDto>> AllAsync(Guid appUserId = default);
    Task<ReviewDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(ReviewDto entity, Guid appUserId = default);
    Task<ReviewDto> UpdateAsync(ReviewDto entity, Guid appUserId = default);
    void Remove(ReviewDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
