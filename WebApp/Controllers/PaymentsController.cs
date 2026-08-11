using Base.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.ViewModels.Payment;

namespace WebApp.Controllers;

[Authorize]
public class PaymentsController : AppControllerBase
{
    private readonly IOrdersModuleApi _bll;
    private readonly IIdentityModuleApi _identity;

    public PaymentsController(IOrdersModuleApi bll, IIdentityModuleApi identity)
    {
        _bll = bll;
        _identity = identity;
    }

    public async Task<IActionResult> Index()
    {
        if (IsRoot()) return RedirectToAction("Index", "Payments", new { area = "Admin" });

        var payments = (await _bll.GetPaymentsAsync()).ToList();
        var accessibleOrderIds = (await GetAccessibleOrdersAsync()).Select(o => o.Id).ToHashSet();
        return View(payments.Where(p => accessibleOrderIds.Contains(p.OrderId)).ToList());
    }

    public async Task<IActionResult> Details(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Details", "Payments", new { area = "Admin", id });
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();
        if (!await CanAccessOrderAsync(payment.OrderId)) return NotFound();

        return View(payment);
    }

    public async Task<IActionResult> Create()
    {
        if (IsRoot()) return RedirectToAction("Create", "Payments", new { area = "Admin" });
        if (!await _identity.IsRestaurantAdminAsync(User.GetUserId())) return Forbid();

        var vm = new PaymentCreateViewModel();
        await PopulateOrderOptionsAsync(vm);

        var firstOrderValue = vm.OrderOptions.FirstOrDefault()?.Value;
        if (Guid.TryParse(firstOrderValue, out var defaultOrderId))
        {
            vm.OrderId = defaultOrderId;
            vm.Amount = (await _bll.GetOrderAsync(defaultOrderId))?.TotalPrice ?? 0m;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PaymentCreateViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Create", "Payments", new { area = "Admin" });
        if (!await CanAdminOrderAsync(vm.OrderId)) return Forbid();

        if (ModelState.IsValid)
        {
            var payment = new Shared.Contracts.Orders.PaymentDto
            {
                Id = Guid.NewGuid(),
                OrderId = vm.OrderId,
                Amount = vm.Amount,
                PaymentStatus = vm.PaymentStatus,
                PaymentMethod = vm.PaymentMethod,
                PaidAt = vm.PaidAt,
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            };

            await _bll.CreatePaymentAsync(payment);
            return RedirectToAction(nameof(Index));
        }

        await PopulateOrderOptionsAsync(vm, vm.OrderId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Payments", new { area = "Admin", id });
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();
        if (!await CanAdminOrderAsync(payment.OrderId)) return NotFound();

        var vm = new PaymentEditViewModel
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentStatus = payment.PaymentStatus,
            PaymentMethod = payment.PaymentMethod,
            PaidAt = payment.PaidAt,
            Summary = payment.Summary
        };

        await PopulateOrderOptionsAsync(vm, vm.OrderId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PaymentEditViewModel vm)
    {
        if (IsRoot()) return RedirectToAction("Edit", "Payments", new { area = "Admin", id });
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindPaymentAsync(id);
        if (entity == null) return NotFound();
        if (!await CanAdminOrderAsync(entity.OrderId) || !await CanAdminOrderAsync(vm.OrderId)) return NotFound();

        if (ModelState.IsValid)
        {
            entity.OrderId = vm.OrderId;
            entity.Amount = vm.Amount;
            entity.PaymentStatus = vm.PaymentStatus;
            entity.PaymentMethod = vm.PaymentMethod;
            entity.PaidAt = vm.PaidAt;
            entity.Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary;

            try
            {
                await _bll.UpdatePaymentDetailsAsync(entity);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _bll.FindPaymentAsync(entity.Id) == null) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await PopulateOrderOptionsAsync(vm, vm.OrderId);
        return View(vm);
    }

    public async Task<IActionResult> Delete(Guid? id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Payments", new { area = "Admin", id });
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();
        if (!await CanAdminOrderAsync(payment.OrderId)) return NotFound();

        return View(payment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (IsRoot()) return RedirectToAction("Delete", "Payments", new { area = "Admin", id });

        var payment = await _bll.FindPaymentAsync(id);
        if (payment == null) return NotFound();
        if (!await CanAdminOrderAsync(payment.OrderId)) return NotFound();

        await _bll.DeletePaymentAsync(payment.Id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> CanAccessOrderAsync(Guid orderId)
    {
        var userId = User.GetUserId();
        var order = await _bll.GetOrderAsync(orderId);
        if (order == null) return false;
        return order.UserId == userId || await _identity.IsRestaurantAdminAsync(userId, order.RestaurantId);
    }

    private async Task<bool> CanAdminOrderAsync(Guid orderId)
    {
        var order = await _bll.GetOrderAsync(orderId);
        return order != null && await _identity.IsRestaurantAdminAsync(User.GetUserId(), order.RestaurantId);
    }

    private async Task<List<Shared.Contracts.Orders.OrderDto>> GetAccessibleOrdersAsync()
    {
        var userId = User.GetUserId();
        var adminRestaurantIds = await _identity.GetAdminRestaurantIdsAsync(userId);
        return (await _bll.GetOrdersAsync())
            .Where(o => o.UserId == userId || adminRestaurantIds.Contains(o.RestaurantId))
            .ToList();
    }

    private async Task<List<Shared.Contracts.Orders.OrderDto>> GetAdminOrdersAsync()
    {
        var restaurantIds = await _identity.GetAdminRestaurantIdsAsync(User.GetUserId());
        return (await _bll.GetOrdersAsync())
            .Where(o => restaurantIds.Contains(o.RestaurantId))
            .ToList();
    }

    private async Task PopulateOrderOptionsAsync(PaymentCreateViewModel vm, Guid? selectedOrderId = null)
    {
        vm.OrderOptions = (await GetAdminOrdersAsync())
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = $"{(o.UserEmail ?? o.UserId.ToString())} - {o.CreatedAt:yyyy-MM-dd HH:mm}",
                Selected = selectedOrderId.HasValue && o.Id == selectedOrderId.Value
            })
            .ToList();
    }

    private async Task PopulateOrderOptionsAsync(PaymentEditViewModel vm, Guid? selectedOrderId = null)
    {
        vm.OrderOptions = (await GetAdminOrdersAsync())
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = $"{(o.UserEmail ?? o.UserId.ToString())} - {o.CreatedAt:yyyy-MM-dd HH:mm}",
                Selected = selectedOrderId.HasValue && o.Id == selectedOrderId.Value
            })
            .ToList();
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderTotalPrice(Guid orderId)
    {
        if (IsRoot()) return RedirectToAction("GetOrderTotalPrice", "Payments", new { area = "Admin", orderId });
        if (!await CanAdminOrderAsync(orderId)) return Forbid();

        var total = (await _bll.GetOrderAsync(orderId))?.TotalPrice;
        if (!total.HasValue) return NotFound();

        return Json(new { totalPrice = total.Value });
    }
}
