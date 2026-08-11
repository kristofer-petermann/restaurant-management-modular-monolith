using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels.OrderRow;

namespace WebApp.Areas.Admin.Controllers;

public class OrderRowsController : AdminBaseController
{
    private readonly IOrdersModuleApi _bll;
    private readonly IRestaurantsModuleApi _restaurants;

    public OrderRowsController(IOrdersModuleApi bll, IRestaurantsModuleApi restaurants)
    {
        _bll = bll;
        _restaurants = restaurants;
    }

    public async Task<IActionResult> Index() => View(await _bll.GetOrderRowsAsync());

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();

        return View(orderRow);
    }

    public async Task<IActionResult> Create()
    {
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
        if (!await IsMenuItemInOrderRestaurantAsync(vm.MenuItemId, vm.OrderId))
        {
            ModelState.AddModelError(nameof(vm.MenuItemId), "Selected menu item must belong to the order's restaurant.");
        }

        if (ModelState.IsValid)
        {
            await _bll.CreateOrderRowAsync(new Shared.Contracts.Orders.OrderRowDto
            {
                Id = Guid.NewGuid(),
                OrderId = vm.OrderId,
                MenuItemId = vm.MenuItemId,
                Quantity = vm.Quantity,
                UnitPrice = vm.UnitPrice,
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            });
            return RedirectToAction(nameof(Index));
        }

        await PopulateOptionsAsync(vm, vm.OrderId, vm.MenuItemId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();

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
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindOrderRowAsync(id);
        if (entity == null) return NotFound();

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
        if (id == null) return NotFound();

        var orderRow = await _bll.FindOrderRowAsync(id.Value);
        if (orderRow == null) return NotFound();

        return View(orderRow);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var orderRow = await _bll.FindOrderRowAsync(id);
        if (orderRow == null) return NotFound();

        await _bll.DeleteOrderRowAsync(orderRow.Id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOptionsAsync(OrderRowCreateViewModel vm, Guid? selectedOrderId = null, Guid? selectedMenuItemId = null)
    {
        var orders = (await _bll.GetOrdersAsync()).OrderByDescending(o => o.CreatedAt).ToList();
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
        vm.OrderOptions = (await _bll.GetOrdersAsync())
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
        var items = await GetMenuItemsForOrderInternalAsync(orderId);
        return items
            .Select(mi => new SelectListItem
            {
                Value = mi.Id.ToString(),
                Text = mi.Name,
                Selected = selectedMenuItemId.HasValue && mi.Id == selectedMenuItemId.Value
            })
            .ToList();
    }

    private async Task<List<Shared.Contracts.Restaurants.MenuItemDto>> GetMenuItemsForOrderInternalAsync(Guid? orderId)
    {
        if (!orderId.HasValue) return new List<Shared.Contracts.Restaurants.MenuItemDto>();

        var order = await _bll.GetOrderAsync(orderId.Value);
        if (order == null) return new List<Shared.Contracts.Restaurants.MenuItemDto>();

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
            .OrderBy(mi => mi.Name)
            .ToList();
    }

    private async Task<bool> IsMenuItemInOrderRestaurantAsync(Guid menuItemId, Guid orderId)
    {
        return (await GetMenuItemsForOrderInternalAsync(orderId)).Any(mi => mi.Id == menuItemId);
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemsForOrder(Guid orderId)
    {
        var items = (await GetMenuItemsForOrderInternalAsync(orderId))
            .Select(mi => new
            {
                value = mi.Id,
                text = mi.Name
            })
            .ToList();

        return Json(items);
    }

    [HttpGet]
    public async Task<IActionResult> GetMenuItemPrice(Guid orderId, Guid menuItemId)
    {
        if (!await IsMenuItemInOrderRestaurantAsync(menuItemId, orderId)) return NotFound();

        var price = (await _restaurants.FindMenuItemAsync(menuItemId))?.Price;
        if (!price.HasValue) return NotFound();

        return Json(new { price = price.Value });
    }
}
