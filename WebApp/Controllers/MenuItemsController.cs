using Base.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Restaurants;
using Shared.Contracts.Restaurants.Commands;
using WebApp.ViewModels.MenuItem;

namespace WebApp.Controllers;

[Authorize]
public class MenuItemsController : AppControllerBase
{
    private readonly IRestaurantsModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IMediator _mediator;

    public MenuItemsController(IRestaurantsModuleApi bll, IIdentityModuleApi identity, IMediator mediator)
    {
        _bll = bll;
        _identity = identity;
        _mediator = mediator;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "MenuItems", new { area = "Admin" });

        var menuItems = (await _bll.GetMenuItemsAsync()).ToList();
        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()))
        {
            var allowedCategoryIds = (await GetAllowedMenuCategoriesAsync()).Select(mc => mc.Id).ToHashSet();
            menuItems = menuItems.Where(mi => allowedCategoryIds.Contains(mi.MenuCategoryId)).ToList();
        }

        return View(menuItems);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "MenuItems", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();

        if (User.Identity?.IsAuthenticated == true &&
            await _identity.IsRestaurantAdminAsync(User.GetUserId()) &&
            !await CanAdminMenuCategoryAsync(menuItem.MenuCategoryId))
        {
            return NotFound();
        }

        return View(menuItem);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "MenuItems", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId())) return Forbid();

        var vm = new MenuItemCreateViewModel();
        await PopulateMenuCategoryOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MenuItemCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "MenuItems", new { area = "Admin" });
        if (!await CanAdminMenuCategoryAsync(vm.MenuCategoryId)) return Forbid();

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
        if (IsRoot()) return RedirectToAction("Edit", "MenuItems", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();
        if (!await CanAdminMenuCategoryAsync(menuItem.MenuCategoryId)) return NotFound();

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
        if (IsRoot()) return RedirectToAction("Edit", "MenuItems", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindMenuItemAsync(id);
        if (entity == null) return NotFound();
        if (!await CanAdminMenuCategoryAsync(entity.MenuCategoryId) ||
            !await CanAdminMenuCategoryAsync(vm.MenuCategoryId))
        {
            return NotFound();
        }

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
        if (IsRoot()) return RedirectToAction("Delete", "MenuItems", new { area = "Admin", id });
        if (id == null) return NotFound();

        var menuItem = await _bll.FindMenuItemAsync(id.Value);
        if (menuItem == null) return NotFound();
        if (!await CanAdminMenuCategoryAsync(menuItem.MenuCategoryId)) return NotFound();

        return View(menuItem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "MenuItems", new { area = "Admin", id });

        var menuItem = await _bll.FindMenuItemAsync(id);
        if (menuItem == null) return NotFound();
        if (!await CanAdminMenuCategoryAsync(menuItem.MenuCategoryId)) return NotFound();

        var removed = await _mediator.Send(new DeleteMenuItemCommand(menuItem.Id));
        if (!removed) return NotFound();
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAdminMenuCategoryAsync(Guid menuCategoryId)
    {
        var menuCategory = await _bll.FindMenuCategoryAsync(menuCategoryId);
        if (menuCategory == null) return false;

        var menu = await _bll.FindMenuAsync(menuCategory.MenuId);
        return menu != null && await _identity.IsRestaurantAdminAsync(User.GetUserId(), menu.RestaurantId);
    }

    private async Task<List<Shared.Contracts.Restaurants.MenuCategoryDto>> GetAllowedMenuCategoriesAsync()
    {
        var restaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
        var menus = (await _bll.GetMenusAsync())
            .Where(m => restaurantIds.Contains(m.RestaurantId))
            .Select(m => m.Id)
            .ToHashSet();

        return (await _bll.GetMenuCategoriesAsync())
            .Where(mc => menus.Contains(mc.MenuId))
            .ToList();
    }

    private async Task PopulateMenuCategoryOptionsAsync(MenuItemCreateViewModel vm, Guid? selectedMenuCategoryId = null)
    {
        vm.MenuCategoryOptions = (await GetAllowedMenuCategoriesAsync())
            .Select(mc => new SelectListItem
            {
                Value = mc.Id.ToString(),
                Text = mc.Name,
                Selected = selectedMenuCategoryId.HasValue && mc.Id == selectedMenuCategoryId.Value
            })
            .ToList();
    }

    private async Task PopulateMenuCategoryOptionsAsync(MenuItemEditViewModel vm, Guid? selectedMenuCategoryId = null)
    {
        vm.MenuCategoryOptions = (await GetAllowedMenuCategoriesAsync())
            .Select(mc => new SelectListItem
            {
                Value = mc.Id.ToString(),
                Text = mc.Name,
                Selected = selectedMenuCategoryId.HasValue && mc.Id == selectedMenuCategoryId.Value
            })
            .ToList();
    }
}
