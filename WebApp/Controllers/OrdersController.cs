using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels.Order;

namespace WebApp.Controllers;

[Authorize]
public class OrdersController : AppControllerBase
{
    private readonly IOrdersModuleApi _bll;
    private readonly IIdentityModuleApi _identity;
    private readonly IRestaurantsModuleApi _restaurants;

    public OrdersController(IOrdersModuleApi bll, IIdentityModuleApi identity, IRestaurantsModuleApi restaurants)
    {
        _bll = bll;
        _identity = identity;
        _restaurants = restaurants;
    }

    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "Orders", new { area = "Admin" });

        var userId = User.GetUserId();
        var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(userId);
        var orders = (await _bll.GetOrdersAsync())
            .Where(o => o.UserId == userId || adminRestaurantIds.Contains(o.RestaurantId))
            .ToList();

        return View(orders);
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "Orders", new { area = "Admin", id });
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();
        if (!await CanAccessOrderAsync(order)) return NotFound();

        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "Orders", new { area = "Admin" });

        var vm = new OrderCreateViewModel { PickupTime = DateTime.Now };
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "Orders", new { area = "Admin" });

        if (ModelState.IsValid)
        {
            var order = new Shared.Contracts.Orders.OrderDto
            {
                Id = Guid.NewGuid(),
                UserId = User.GetUserId(),
                RestaurantId = vm.RestaurantId,
                OrderStatus = vm.OrderStatus,
                TotalPrice = vm.TotalPrice,
                CreatedAt = DateTime.UtcNow,
                PickupTime = NormalizeLocalDateTimeToUtc(vm.PickupTime),
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            };

            await _bll.CreateOrderAsync(order, User.GetUserId());
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Orders", new { area = "Admin", id });
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();
        if (!await CanAccessOrderAsync(order)) return NotFound();

        var vm = new OrderEditViewModel
        {
            Id = order.Id,
            RestaurantId = order.RestaurantId,
            OrderStatus = order.OrderStatus,
            TotalPrice = order.TotalPrice,
            PickupTime = (order.PickupTime ?? order.CreatedAt).ToLocalTime(),
            Summary = order.Summary
        };

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, OrderEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Orders", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.GetOrderAsync(id);
        if (entity == null) return NotFound();
        var accessOrder = await _bll.GetOrderAsync(id);
        if (accessOrder == null || !await CanAccessOrderAsync(accessOrder)) return NotFound();

        if (ModelState.IsValid)
        {
            entity.RestaurantId = vm.RestaurantId;
            entity.OrderStatus = vm.OrderStatus;
            entity.TotalPrice = vm.TotalPrice;
            entity.PickupTime = NormalizeLocalDateTimeToUtc(vm.PickupTime);
            entity.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary;

            try
            {
                await _bll.UpdateOrderDetailsAsync(entity, User.GetUserId());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.GetOrderAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Orders", new { area = "Admin", id });
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();
        if (!await CanAccessOrderAsync(order)) return NotFound();

        return View(order);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Orders", new { area = "Admin", id });

        var order = await _bll.GetOrderAsync(id);
        if (order == null) return NotFound();
        if (!await CanAccessOrderAsync(order)) return NotFound();

        await _bll.DeleteOrderAsync(id, User.GetUserId());
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAccessOrderAsync(Shared.Contracts.Orders.OrderDto order)
    {
        var userId = User.GetUserId();
        return order.UserId == userId || await _identity.IsRestaurantAdminAsync(userId, order.RestaurantId);
    }

    private static DateTime NormalizeLocalDateTimeToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc) return value;
        if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
    }

    private async Task PopulateRestaurantOptionsAsync(OrderCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await _restaurants.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }

    private async Task PopulateRestaurantOptionsAsync(OrderEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = (await _restaurants.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }
}
