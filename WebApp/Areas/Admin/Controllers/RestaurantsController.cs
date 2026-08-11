using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Restaurants.Commands;
using WebApp.Areas.Admin.ViewModels.Restaurant;
using Shared.Contracts.Restaurants;

namespace WebApp.Areas.Admin.Controllers;

public class RestaurantsController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public RestaurantsController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _bll.GetRestaurantsAsync());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

        return View(restaurant);
    }

    public IActionResult Create()
    {
        return View(new RestaurantCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RestaurantCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        await _mediator.Send(new CreateRestaurantCommand(
            vm.RestaurantName,
            string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description,
            string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone,
            string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email,
            string.IsNullOrWhiteSpace(vm.Summary) ? null : vm.Summary));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

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
        if (!ModelState.IsValid) return View(vm);

        var entity = await _bll.FindRestaurantAsync(id);
        if (entity == null) return NotFound();

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

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var restaurant = await _bll.FindRestaurantAsync(id.Value);
        if (restaurant == null) return NotFound();

        return View(restaurant);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _bll.FindRestaurantAsync(id);
        if (entity == null) return NotFound();

        var removed = await _mediator.Send(new DeleteRestaurantCommand(entity.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
