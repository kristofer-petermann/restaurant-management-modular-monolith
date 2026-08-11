using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels.OrderRow;

namespace WebApp.Controllers;

[Authorize]
public class OrderRowsController : AppControllerBase
{
    private readonly IOrdersModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IRestaurantsModuleApi _restaurants;

    public OrderRowsController(IOrdersModuleApi bll, IIdentityModuleApi identity, IRestaurantsModuleApi restaurants)
    {
        _bll = bll;
        _identity = identity;
        _restaurants = restaurants;
    }

    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "OrderRows", new { area = "Admin" });

        var accessibleOrderIds = (await GetAccessibleOrdersAsync()).Select(o => o.Id).ToHashSet();
        var orderRows = (await _bll.GetOrderRowsAsync())
            .Where(or => accessibleOrderIds.Contains(or.OrderId))
            .ToList();

        return View(orderRows);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "OrderRows", new { area = "Admin", id });
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();
        if (!await CanAccessOrderAsync(orderRow.OrderId)) return NotFound();

        return View(orderRow);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "OrderRows", new { area = "Admin" });

        var vm = new OrderRowCreateViewModel();
        await PopulateOptionsAsync(vm);

        var firstMenuItemValue = vm.MenuItemOptions.FirstOrDefault()?.Value;
        if (vm.MenuItemId == Guid.Empty && Guid.TryParse(firstMenuItemValue, out var defaultMenuItemId))
        {
            vm.MenuItemId = defaultMenuItemId;
            vm.UnitPrice = (await _restaurants.FindMenuItemAsync(defaultMenuItemId))?.Price ?? 0m;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderRowCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "OrderRows", new { area = "Admin" });
        if (!await CanAccessOrderAsync(vm.OrderId)) return Forbid();

        if (!await IsMenuItemInOrderRestaurantAsync(vm.MenuItemId, vm.OrderId))
        {
            ModelState.AddModelError(nameof(vm.MenuItemId), "Selected menu item must belong to the order's restaurant.");
        }

        if (ModelState.IsValid)
        {
            var orderRow = new Shared.Contracts.Orders.OrderRowDto
            {
                Id = Guid.NewGuid(),
                OrderId = vm.OrderId,
                MenuItemId = vm.MenuItemId,
                Quantity = vm.Quantity,
                UnitPrice = vm.UnitPrice,
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            };

            await _bll.CreateOrderRowAsync(orderRow);
            return RedirectToAction(nameof(Index));
        }

        await PopulateOptionsAsync(vm, vm.OrderId, vm.MenuItemId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "OrderRows", new { area = "Admin", id });
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();
        if (!await CanAccessOrderAsync(orderRow.OrderId)) return NotFound();

        var vm = new OrderRowEditViewModel
        {
            Id = orderRow.Id,
            OrderId = orderRow.OrderId,
            MenuItemId = orderRow.MenuItemId,
            Quantity = orderRow.Quantity,
            UnitPrice = orderRow.UnitPrice,
            Summary = orderRow.Summary
        };

        await PopulateOptionsAsync(vm, vm.OrderId, vm.MenuItemId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OrderRowEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "OrderRows", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindOrderRowAsync(id);
        if (entity == null) return NotFound();
        if (!await CanAccessOrderAsync(entity.OrderId) || !await CanAccessOrderAsync(vm.OrderId)) return NotFound();

        if (!await IsMenuItemInOrderRestaurantAsync(vm.MenuItemId, vm.OrderId))
        {
            ModelState.AddModelError(nameof(vm.MenuItemId), "Selected menu item must belong to the order's restaurant.");
        }

        if (ModelState.IsValid)
        {
            entity.OrderId = vm.OrderId;
            entity.MenuItemId = vm.MenuItemId;
            entity.Quantity = vm.Quantity;
            entity.UnitPrice = vm.UnitPrice;
            entity.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary;

            try
            {
                await _bll.UpdateOrderRowDetailsAsync(entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindOrderRowAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateOptionsAsync(vm, vm.OrderId, vm.MenuItemId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "OrderRows", new { area = "Admin", id });
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();
        if (!await CanAccessOrderAsync(orderRow.OrderId)) return NotFound();

        return View(orderRow);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "OrderRows", new { area = "Admin", id });

        var orderRow = await _bll.FindOrderRowAsync(id);
        if (orderRow == null) return NotFound();
        if (!await CanAccessOrderAsync(orderRow.OrderId)) return NotFound();

        await _bll.DeleteOrderRowAsync(orderRow.Id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAccessOrderAsync(Guid orderId)
    {
        var userId = User.GetUserId();
        var order = await _bll.GetOrderAsync(orderId);
        if (order == null) return false;
        return order.UserId == userId || await _identity.IsRestaurantAdminAsync(userId, order.RestaurantId);
    }

    private async Task<List<Shared.Contracts.Orders.OrderDto>> GetAccessibleOrdersAsync()
    {
        var userId = User.GetUserId();
        var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(userId);
        return (await _bll.GetOrdersAsync())
            .Where(o => o.UserId == userId || adminRestaurantIds.Contains(o.RestaurantId))
            .OrderByDescending(o => o.CreatedAt)
            .ToList();
    }

    private async Task PopulateOptionsAsync(OrderRowCreateViewModel vm, Guid? selectedOrderId = null, Guid? selectedMenuItemId = null)
    {
        var orders = await GetAccessibleOrdersAsync();
        var effectiveOrderId = selectedOrderId ?? orders.Select(o => (Guid?)o.Id).FirstOrDefault();

        vm.OrderOptions = orders.Select(o => new SelectListItem
        {
            Value = o.Id.ToString(),
            Text = $"{(o.UserEmail ?? o.UserId.ToString())} - {o.CreatedAt:yyyy-MM-dd HH:mm}",
            Selected = effectiveOrderId.HasValue && o.Id == effectiveOrderId.Value
        }).ToList();

        if (effectiveOrderId.HasValue) vm.OrderId = effectiveOrderId.Value;
        vm.MenuItemOptions = await GetMenuItemOptionsForOrderAsync(effectiveOrderId, selectedMenuItemId);
    }

    private async Task PopulateOptionsAsync(OrderRowEditViewModel vm, Guid? selectedOrderId = null, Guid? selectedMenuItemId = null)
    {
        vm.OrderOptions = (await GetAccessibleOrdersAsync())
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = $"{(o.UserEmail ?? o.UserId.ToString())} - {o.CreatedAt:yyyy-MM-dd HH:mm}",
                Selected = selectedOrderId.HasValue && o.Id == selectedOrderId.Value
            })
            .ToList();

        vm.MenuItemOptions = await GetMenuItemOptionsForOrderAsync(selectedOrderId, selectedMenuItemId);
    }

    private async Task<List<SelectListItem>> GetMenuItemOptionsForOrderAsync(Guid? orderId, Guid? selectedMenuItemId = null)
    {
        if (!orderId.HasValue) return new List<SelectListItem>();

        var order = await _bll.GetOrderAsync(orderId.Value);
        if (order == null) return new List<SelectListItem>();

        var menuIds = (await _restaurants.GetMenusAsync())
            .Where(m => m.RestaurantId == order.RestaurantId)
            .Select(m => m.Id)
            .ToHashSet();
        var categoryIds = (await _restaurants.GetMenuCategoriesAsync())
            .Where(mc => menuIds.Contains(mc.MenuId))
            .Select(mc => mc.Id)
            .ToHashSet();

        return (await _restaurants.GetMenuItemsAsync())
            .Where(mi => categoryIds.Contains(mi.MenuCategoryId))
            .Select(mi => new SelectListItem
            {
                Value = mi.Id.ToString(),
                Text = mi.Name,
                Selected = selectedMenuItemId.HasValue && mi.Id == selectedMenuItemId.Value
            })
            .ToList();
    }

    private async Task<bool> IsMenuItemInOrderRestaurantAsync(Guid menuItemId, Guid orderId)
    {
        var options = await GetMenuItemOptionsForOrderAsync(orderId);
        return options.Any(o => o.Value == menuItemId.ToString());
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemsForOrder(Guid orderId)
    {
        if (IsRoot()) return RedirectToAction("GetMenuItemsForOrder", "OrderRows", new { area = "Admin", orderId });
        if (!await CanAccessOrderAsync(orderId)) return Forbid();

        var items = (await GetMenuItemOptionsForOrderAsync(orderId))
            .Select(o => new { value = o.Value, text = o.Text })
            .ToList();

        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemPrice(Guid orderId, Guid menuItemId)
    {
        if (IsRoot()) return RedirectToAction("GetMenuItemPrice", "OrderRows", new { area = "Admin", orderId, menuItemId });
        if (!await CanAccessOrderAsync(orderId)) return Forbid();
        if (!await IsMenuItemInOrderRestaurantAsync(menuItemId, orderId)) return NotFound();

        var price = (await _restaurants.FindMenuItemAsync(menuItemId))?.Price;
        if (!price.HasValue) return NotFound();

        return Json(new { price = price.Value });
    }
}
