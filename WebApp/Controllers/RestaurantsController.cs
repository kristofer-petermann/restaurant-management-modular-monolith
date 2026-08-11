using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.Restaurant;

namespace WebApp.Controllers;

[Authorize]
public class RestaurantsController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public RestaurantsController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        var restaurants = (await _bll.GetRestaurantsAsync()).ToList();

        if (User.Identity?.IsAuthenticated == true && !IsRoot())
        {
            var userId = User.GetUserId();
            if (await _identity.IsRestaurantAdminAsync(userId))
            {
                var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(userId);
                restaurants = restaurants.Where(r => adminRestaurantIds.Contains(r.Id)).ToList();
            }
        }

        return View(restaurants);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            !IsRoot() &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await _identity.IsRestaurantAdminAsync(User.GetUserId(), restaurant.Id))
        {
            return NotFound();
        }

        return View(restaurant);
    }

    public IActionResult Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "Restaurants", new { area = "Admin" });
        return Forbid();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(RestaurantCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "Restaurants", new { area = "Admin" });
        return Forbid();
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();
        if (IsRoot()) return RedirectToAction("Edit", "Restaurants", new { area = "Admin", id });

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), restaurant.Id))
        {
            return NotFound();
        }

        var vm = new RestaurantEditViewModel
        {
            Id = restaurant.Id,
            RestaurantName = restaurant.RestaurantName,
            Description = restaurant.Description,
            Phone = restaurant.Phone,
            Email = restaurant.Email,
            Summary = restaurant.Summary
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, RestaurantEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();
        if (IsRoot()) return RedirectToAction("Edit", "Restaurants", new { area = "Admin", id });

        var entity = await _bll.FindRestaurantAsync(id);
        if (entity == null) return NotFound();

        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), entity.Id))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateRestaurantCommand(
                    entity.Id,
                    vm.RestaurantName,
                    string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description,
                    string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone,
                    string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email,
                    string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindRestaurantAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();
        if (IsRoot()) return RedirectToAction("Delete", "Restaurants", new { area = "Admin", id });

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), restaurant.Id))
        {
            return NotFound();
        }

        return View(restaurant);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Restaurants", new { area = "Admin", id });

        var restaurant = await _bll.FindRestaurantAsync(id);
        if (restaurant == null) return NotFound();

        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId(), restaurant.Id))
        {
            return NotFound();
        }

        var removed = await _mediator.Send(new DeleteRestaurantCommand(restaurant.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
