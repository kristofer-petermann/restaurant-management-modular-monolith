using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Restaurants.Commands;
using WebApp.Areas.Admin.ViewModels.Menu;
using Shared.Contracts.Restaurants;

namespace WebApp.Areas.Admin.Controllers;

public class MenusController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public MenusController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _bll.GetMenusAsync());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();

        return View(menu);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new MenuCreateViewModel();
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
            return View(vm);
        }

        await _mediator.Send(new CreateMenuCommand(
            vm.RestaurantId,
            vm.Name,
            vm.IsActive,
            string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();

        var vm = new MenuEditViewModel
        {
            Id = menu.Id,
            RestaurantId = menu.RestaurantId,
            Name = menu.Name,
            IsActive = menu.IsActive,
            Summary = menu.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindMenuAsync(id);
        if (entity == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
            return View(vm);
        }

        try
        {
            var updated = await _mediator.Send(new UpdateMenuCommand(
                entity.Id,
                vm.RestaurantId,
                vm.Name,
                vm.IsActive,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            if (updated == null) return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (await _bll.FindMenuAsync(entity.Id) == null) return NotFound();
            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var menu = await _bll.FindMenuAsync(id.Value);
        if (menu == null) return NotFound();

        return View(menu);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var entity = await _bll.FindMenuAsync(id);
        if (entity == null) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuCommand(entity.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRestaurantOptionsAsync(MenuCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task PopulateRestaurantOptionsAsync(MenuEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task<List<SelectListItem>> BuildRestaurantOptionsAsync(Guid? selectedRestaurantId = null)
    {
        return (await _bll.GetRestaurantsAsync())
            .OrderBy(r => r.RestaurantName)
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }
}
