using Modules.Restaurants.Domain;

namespace Modules.Restaurants.Infrastructure;

internal interface IOpeningHourRepository
{
    Task<List<OpeningHour>> GetAllAsync(Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<OpeningHour?> FindByIdAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    Task<OpeningHour?> FindTrackedByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OpeningHour?> FindForDeleteAsync(Guid id, Guid appUserId = default, CancellationToken cancellationToken = default);
    void Add(OpeningHour entity);
    void Remove(OpeningHour entity);
}
