using Base.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels.Order;

namespace WebApp.Areas.Admin.Controllers;

public class OrdersController : AdminBaseController
{
    private readonly IOrdersModuleApi _bll;
    private readonly IRestaurantsModuleApi _restaurants;

    public OrdersController(IOrdersModuleApi bll, IRestaurantsModuleApi restaurants)
    {
        _bll = bll;
        _restaurants = restaurants;
    }

    public async Task<IActionResult> Index() => View(await _bll.GetOrdersAsync());

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();

        return View(order);
    }

    public async Task<IActionResult> Create()
    {
        var vm = new OrderCreateViewModel { PickupTime = DateTime.Now };
        await PopulateRestaurantOptionsAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateViewModel vm)
    {
        if (ModelState.IsValid)
        {
            await _bll.CreateOrderAsync(new Shared.Contracts.Orders.OrderDto
            {
                Id = Guid.NewGuid(),
                UserId = User.GetUserId(),
                RestaurantId = vm.RestaurantId,
                OrderStatus = vm.OrderStatus,
                TotalPrice = vm.TotalPrice,
                CreatedAt = DateTime.UtcNow,
                PickupTime = NormalizeLocalDateTimeToUtc(vm.PickupTime),
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            }, User.GetUserId());
            return RedirectToAction(nameof(Index));
        }

        await PopulateRestaurantOptionsAsync(vm, vm.RestaurantId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();

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
        if (id != vm.Id) return NotFound();

        var entity = await _bll.GetOrderAsync(id);
        if (entity == null) return NotFound();

        if (ModelState.IsValid)
        {
            entity.RestaurantId = vm.RestaurantId;
            entity.OrderStatus = vm.OrderStatus;
            entity.TotalPrice = vm.TotalPrice;
            entity.PickupTime = NormalizeLocalDateTimeToUtc(vm.PickupTime);
            entity.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary;

            try
            {
                await _bll.UpdateOrderDetailsAsync(entity);
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
        if (id == null) return NotFound();

        var order = await _bll.GetOrderAsync(id.Value);
        if (order == null) return NotFound();

        return View(order);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var order = await _bll.GetOrderAsync(id);
        if (order == null) return NotFound();

        await _bll.DeleteOrderAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static DateTime NormalizeLocalDateTimeToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Utc) return value;
        if (value.Kind == DateTimeKind.Local) return value.ToUniversalTime();
        return DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime();
    }

    private async Task PopulateRestaurantOptionsAsync(OrderCreateViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task PopulateRestaurantOptionsAsync(OrderEditViewModel vm, Guid? selectedRestaurantId = null)
    {
        vm.RestaurantOptions = await BuildRestaurantOptionsAsync(selectedRestaurantId);
    }

    private async Task<List<SelectListItem>> BuildRestaurantOptionsAsync(Guid? selectedRestaurantId = null)
    {
        return (await _restaurants.GetRestaurantsAsync())
            .Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = r.RestaurantName,
                Selected = selectedRestaurantId.HasValue && r.Id == selectedRestaurantId.Value
            })
            .ToList();
    }
}
