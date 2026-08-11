using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Shared.Contracts.Restaurants.Commands;
using WebApp.Areas.Admin.ViewModels.MenuCategory;
using Shared.Contracts.Restaurants;

namespace WebApp.Areas.Admin.Controllers;

public class MenuCategoriesController : AdminBaseController
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IMediator _mediator;

    public MenuCategoriesController(IRestaurantsModuleApi bll, IMediator mediator)
    {
        _bll = bll;
        _mediator = mediator;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _bll.GetMenuCategoriesAsync());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();

        return View(menuCategory);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new MenuCategoryCreateViewModel();
        await PopulateMenuOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuCategoryCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _mediator.Send(new CreateMenuCategoryCommand(
                vm.MenuId,
                vm.Name,
                string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
            return RedirectToAction(nameof(Index));
        }

        await PopulateMenuOptionsAsync(vm, vm.MenuId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();

        var vm = new MenuCategoryEditViewModel
        {
            Id = menuCategory.Id,
            MenuId = menuCategory.MenuId,
            Name = menuCategory.Name,
            Summary = menuCategory.Summary
        };

        await PopulateMenuOptionsAsync(vm, vm.MenuId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MenuCategoryEditViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindMenuCategoryAsync(id);
        if (entity == null) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var updated = await _mediator.Send(new UpdateMenuCategoryCommand(
                    entity.Id,
                    vm.MenuId,
                    vm.Name,
                    string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary));
                if (updated == null) return NotFound();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindMenuCategoryAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateMenuOptionsAsync(vm, vm.MenuId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();

        return View(menuCategory);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var menuCategory = await _bll.FindMenuCategoryAsync(id);
        if (menuCategory == null) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuCategoryCommand(menuCategory.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMenuOptionsAsync(MenuCategoryCreateViewModel vm, Guid? selectedMenuId = null)
    {
        vm.MenuOptions = await BuildMenuOptionsAsync(selectedMenuId);
    }

    private async Task PopulateMenuOptionsAsync(MenuCategoryEditViewModel vm, Guid? selectedMenuId = null)
    {
        vm.MenuOptions = await BuildMenuOptionsAsync(selectedMenuId);
    }

    private async Task<List<SelectListItem>> BuildMenuOptionsAsync(Guid? selectedMenuId = null)
    {
        return (await _bll.GetMenusAsync())
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
                Selected = selectedMenuId.HasValue && m.Id == selectedMenuId.Value
            })
            .ToList();
    }
}
