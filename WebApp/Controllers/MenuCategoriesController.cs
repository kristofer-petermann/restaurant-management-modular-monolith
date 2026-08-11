using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.MenuCategory;

namespace WebApp.Controllers;

[Authorize]
public class MenuCategoriesController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public MenuCategoriesController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "MenuCategories", new { area = "Admin" });

        var menuCategories = (await _bll.GetMenuCategoriesAsync()).ToList();
        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            var allowedMenuIds = (await GetAllowedMenusAsync()).Select(m => m.Id).ToHashSet();
            menuCategories = menuCategories.Where(mc => allowedMenuIds.Contains(mc.MenuId)).ToList();
        }

        return View(menuCategories);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "MenuCategories", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await CanAdminMenuAsync(menuCategory.MenuId))
        {
            return NotFound();
        }

        return View(menuCategory);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "MenuCategories", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId())) return Forbid();

        var vm = new MenuCategoryCreateViewModel();
        await PopulateMenuOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuCategoryCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "MenuCategories", new { area = "Admin" });
        if (!await CanAdminMenuAsync(vm.MenuId)) return Forbid();

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
        if (IsRoot()) return RedirectToAction("Edit", "MenuCategories", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();
        if (!await CanAdminMenuAsync(menuCategory.MenuId)) return NotFound();

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
        if (IsRoot()) return RedirectToAction("Edit", "MenuCategories", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindMenuCategoryAsync(id);
        if (entity == null) return NotFound();
        if (!await CanAdminMenuAsync(entity.MenuId) || !await CanAdminMenuAsync(vm.MenuId)) return NotFound();

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
        if (IsRoot()) return RedirectToAction("Delete", "MenuCategories", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuCategory = await _bll.FindMenuCategoryAsync(id.Value);
        if (menuCategory == null) return NotFound();
        if (!await CanAdminMenuAsync(menuCategory.MenuId)) return NotFound();

        return View(menuCategory);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "MenuCategories", new { area = "Admin", id });

        var menuCategory = await _bll.FindMenuCategoryAsync(id);
        if (menuCategory == null) return NotFound();
        if (!await CanAdminMenuAsync(menuCategory.MenuId)) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuCategoryCommand(menuCategory.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAdminMenuAsync(Guid menuId)
    {
        var menu = await _bll.FindMenuAsync(menuId);
        return menu != null && await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId);
    }

    private async Task<List<MenuDto>> GetAllowedMenusAsync()
    {
        var restaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
        return (await _bll.GetMenusAsync())
            .Where(m => restaurantIds.Contains(m.RestaurantId))
            .ToList();
    }

    private async Task PopulateMenuOptionsAsync(MenuCategoryCreateViewModel vm, Guid? selectedMenuId = null)
    {
        vm.MenuOptions = (await GetAllowedMenusAsync())
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
                Selected = selectedMenuId.HasValue && m.Id == selectedMenuId.Value
            })
            .ToList();
    }

    private async Task PopulateMenuOptionsAsync(MenuCategoryEditViewModel vm, Guid? selectedMenuId = null)
    {
        vm.MenuOptions = (await GetAllowedMenusAsync())
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name,
                Selected = selectedMenuId.HasValue && m.Id == selectedMenuId.Value
            })
            .ToList();
    }
}
