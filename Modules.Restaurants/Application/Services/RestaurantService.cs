using Microsoft.EntityFrameworkCore;
using Base.Domain;
using Shared.Contracts.Restaurants;
using Modules.Restaurants.Infrastructure;

namespace Modules.Restaurants.Application.Services;

internal class RestaurantService : Modules.Restaurants.Application.Services.IRestaurantService
{
    private readonly IRestaurantRepository _repository;

    public RestaurantService(IRestaurantRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<RestaurantDto>> AllAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<IEnumerable<RestaurantReadDto>> AllReadAsync(Guid appUserId = default)
    {
        return (await _repository.GetAllAsync(appUserId)).Select(ToReadDto).ToList();
    }

    public async Task<RestaurantDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public async Task<RestaurantReadDto?> FindReadAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToReadDto(entity);
    }

    public void Add(RestaurantDto entity, Guid appUserId = default)
    {
        _repository.Add(ToDomain(entity));
    }

    public async Task<RestaurantDto> UpdateAsync(RestaurantDto entity, Guid appUserId = default)
    {
        var dbEntity = await _repository.FindTrackedByIdAsync(entity.Id)
            ?? throw new DbUpdateConcurrencyException($"Restaurant {entity.Id} was not found.");
        dbEntity.RestaurantName = entity.RestaurantName;
        dbEntity.Description = entity.Description ?? string.Empty;
        dbEntity.Phone = entity.Phone;
        dbEntity.Email = entity.Email;
        dbEntity.Summary = entity.Summary ?? string.Empty;
        return ToDto(dbEntity);
    }

    public void Remove(RestaurantDto entity)
    {
        _repository.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _repository.FindForDeleteAsync(id, appUserId);
        if (entity != null) _repository.Remove(entity);
    }

    private static RestaurantDto ToDto(Modules.Restaurants.Domain.Restaurant entity)
    {
        return new RestaurantDto
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description ?? string.Empty,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary
        };
    }

    private static Modules.Restaurants.Domain.Restaurant ToDomain(RestaurantDto entity)
    {
        return new Modules.Restaurants.Domain.Restaurant
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description ?? string.Empty,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static RestaurantReadDto ToReadDto(Modules.Restaurants.Domain.Restaurant entity)
    {
        return new RestaurantReadDto
        {
            Id = entity.Id,
            RestaurantName = entity.RestaurantName,
            Description = entity.Description,
            Phone = entity.Phone,
            Email = entity.Email,
            Summary = entity.Summary
        };
    }

}
