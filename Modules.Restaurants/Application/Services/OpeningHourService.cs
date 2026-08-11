using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class OpeningHourService : Modules.Restaurants.Application.Services.IOpeningHourService
{
    private readonly IOpeningHourRepository _repository;

    public OpeningHourService(IOpeningHourRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<OpeningHourDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<OpeningHourReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<OpeningHourDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<OpeningHourReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(OpeningHourDto entity, Guid appUserId = default)
    {
        _repository.Add(ToDomain(entity));
    }

    public async Task<OpeningHourDto> UpdateAsync(OpeningHourDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Opening hour {entity.Id} was not found.");
        dbEntity.RestaurantId = entity.RestaurantId;
        dbEntity.DayOfWeek = UpsertTranslation(dbEntity.DayOfWeek, entity.DayOfWeek);
        dbEntity.OpenTime = entity.OpenTime;
        dbEntity.CloseTime = entity.CloseTime;
        dbEntity.Summary = UpsertTranslation(dbEntity.Summary, entity.Summary);
        return ToDto(dbEntity);
    }

    public void Remove(OpeningHourDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static OpeningHourDto ToDto(Modules.Restaurants.Domain.OpeningHour entity)
    {
        return new OpeningHourDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary,
            Restaurant = entity.Restaurant == null
                ? null
                : new OpeningHourRestaurantDto
                {
                    Id = entity.Restaurant.Id,
                    RestaurantName = entity.Restaurant.RestaurantName
                }
        };
    }

    private static Modules.Restaurants.Domain.OpeningHour ToDomain(OpeningHourDto entity)
    {
        return new Modules.Restaurants.Domain.OpeningHour
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek ?? string.Empty,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static OpeningHourReadDto ToReadDto(Modules.Restaurants.Domain.OpeningHour entity)
    {
        return new OpeningHourReadDto
        {
            Id = entity.Id,
            RestaurantId = entity.RestaurantId,
            DayOfWeek = entity.DayOfWeek,
            OpenTime = entity.OpenTime,
            CloseTime = entity.CloseTime,
            Summary = entity.Summary,
            RestaurantName = entity.Restaurant?.RestaurantName
        };
    }

    private static LangStr UpsertTranslation(LangStr? existing, string? value)
    {
        var val = value ?? string.Empty;
        existing ??= new LangStr(val);
        existing.SetTranslation(val);
        return existing;
    }
}
