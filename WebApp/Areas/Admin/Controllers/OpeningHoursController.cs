using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Restaurants.Commands;
using WebApp.Areas.Admin.ViewModels.OpeningHour;
using Shared.Contracts.Restaurants;

namespace WebApp.Areas.Admin.Controllers;

public class OpeningHoursController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public OpeningHoursController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index() => View(await _bll.GetOpeningHoursAsync());

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();

        return View(openingHour);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new OpeningHourCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OpeningHourCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateOpeningHourCommand(
                vm.RestaurantId,
                vm.DayOfWeek,
                vm.OpenTime,
                vm.CloseTime,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();

        var vm = new OpeningHourEditViewModel
        {
            Id = openingHour.Id,
            RestaurantId = openingHour.RestaurantId,
            DayOfWeek = openingHour.DayOfWeek,
            OpenTime = openingHour.OpenTime,
            CloseTime = openingHour.CloseTime,
            Summary = openingHour.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OpeningHourEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindOpeningHourAsync(id);
        if (entity == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateOpeningHourCommand(
                    entity.Id,
                    vm.RestaurantId,
                    vm.DayOfWeek,
                    vm.OpenTime,
                    vm.CloseTime,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindOpeningHourAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var openingHour = await _bll.FindOpeningHourAsync(id.Value);
        if (openingHour == null) return NotFound();

        return View(openingHour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var openingHour = await _bll.FindOpeningHourAsync(id);
        if (openingHour == null) return NotFound();

        var removed = await _mediator.Send(new DeleteOpeningHourCommand(openingHour.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(OpeningHourCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task PopulateRestaurantOptionsAsync(OpeningHourEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task<List<SelectListItem>> BuildRestaurantOptionsAsync(Guid? selectedRestaurantId = null)
    {
        return (await _bll.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }
}
