using Shared.Contracts.Restaurants;

namespace Modules.Restaurants.Application.Services;

internal interface IOpeningHourService
{
    Task<IEnumerable<OpeningHourReadDto>> AllReadAsync(Guid appUserId = default);
    Task<OpeningHourReadDto?> FindReadAsync(Guid id, Guid appUserId = default);

    Task<IEnumerable<OpeningHourDto>> AllAsync(Guid appUserId = default);
    Task<OpeningHourDto?> FindAsync(Guid id, Guid appUserId = default);
    void Add(OpeningHourDto entity, Guid appUserId = default);
    Task<OpeningHourDto> UpdateAsync(OpeningHourDto entity, Guid appUserId = default);
    void Remove(OpeningHourDto entity);
    Task RemoveAsync(Guid id, Guid appUserId = default);
}
