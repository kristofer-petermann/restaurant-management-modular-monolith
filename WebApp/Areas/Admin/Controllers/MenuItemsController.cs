using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Restaurants.Commands;
using WebApp.Areas.Admin.ViewModels.MenuItem;
using Shared.Contracts.Restaurants;

namespace WebApp.Areas.Admin.Controllers;

public class MenuItemsController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public MenuItemsController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _bll.GetMenuItemsAsync());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();

        return View(menuItem);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new MenuItemCreateViewModel();
        await PopulateMenuCategoryOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateMenuItemCommand(
                vm.MenuCategoryId,
                vm.Name,
                string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description,
                vm.Price,
                vm.IsAvailable,
                string.IsNullOrWhiteSpace(vm.ImageUrl) ? null : vm.ImageUrl,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            return RedirectToAction(nameof(Index));
        }

        await PopulateMenuCategoryOptionsAsync(vm, vm.MenuCategoryId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();

        var vm = new MenuItemEditViewModel
        {
            Id = menuItem.Id,
            MenuCategoryId = menuItem.MenuCategoryId,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            IsAvailable = menuItem.IsAvailable,
            ImageUrl = menuItem.ImageUrl,
            Summary = menuItem.Summary
        };

        await PopulateMenuCategoryOptionsAsync(vm, vm.MenuCategoryId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuItemEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindMenuItemAsync(id);
        if (entity == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateMenuItemCommand(
                    entity.Id,
                    vm.MenuCategoryId,
                    vm.Name,
                    string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description,
                    vm.Price,
                    vm.IsAvailable,
                    string.IsNullOrWhiteSpace(vm.ImageUrl) ? null : vm.ImageUrl,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindMenuItemAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateMenuCategoryOptionsAsync(vm, vm.MenuCategoryId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();

        return View(menuItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var menuItem = await _bll.FindMenuItemAsync(id);
        if (menuItem == null) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuItemCommand(menuItem.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMenuCategoryOptionsAsync(MenuItemCreateViewModel vm, Guid? selectedMenuCategoryId = null)
    {
        vm.MenuCategoryOptions = await BuildMenuCategoryOptionsAsync(selectedMenuCategoryId);
    }

    private async Task PopulateMenuCategoryOptionsAsync(MenuItemEditViewModel vm, Guid? selectedMenuCategoryId = null)
    {
        vm.MenuCategoryOptions = await BuildMenuCategoryOptionsAsync(selectedMenuCategoryId);
    }

    private async Task<List<SelectListItem>> BuildMenuCategoryOptionsAsync(Guid? selectedMenuCategoryId = null)
    {
        return (await _bll.GetMenuCategoriesAsync())
            .Select(mc => new SelectListItem
            {
                Value = mc.Id.ToString(),
                Text = mc.Name,
                Selected = selectedMenuCategoryId.HasValue && mc.Id == selectedMenuCategoryId.Value
            })
            .ToList();
    }
}
