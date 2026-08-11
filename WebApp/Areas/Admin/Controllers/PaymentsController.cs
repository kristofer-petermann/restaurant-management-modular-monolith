using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Areas.Admin.ViewModels.Payment;

namespace WebApp.Areas.Admin.Controllers;

public class PaymentsController : AdminBaseController
{
    private readonly IOrdersModuleApi _bll;

    public PaymentsController(IOrdersModuleApi bll)
    {
        _bll = bll;
    }

    public async Task<IActionResult> Index() => View(await _bll.GetPaymentsAsync());

    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();

        return View(payment);
    }

    public async Task<IActionResult> Create()
    {
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
        if (ModelState.IsValid)
        {
            await _bll.CreatePaymentAsync(new Shared.Contracts.Orders.PaymentDto
            {
                Id = Guid.NewGuid(),
                OrderId = vm.OrderId,
                Amount = vm.Amount,
                PaymentStatus = vm.PaymentStatus,
                PaymentMethod = vm.PaymentMethod,
                PaidAt = vm.PaidAt,
                Summary = string.IsNullOrWhiteSpace(vm.Summary) ? string.Empty : vm.Summary
            });
            return RedirectToAction(nameof(Index));
        }

        await PopulateOrderOptionsAsync(vm, vm.OrderId);
        return View(vm);
    }

    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();

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
        if (id != vm.Id) return NotFound();

        var entity = await _bll.FindPaymentAsync(id);
        if (entity == null) return NotFound();

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
        if (id == null) return NotFound();

        var payment = await _bll.FindPaymentAsync(id.Value);
        if (payment == null) return NotFound();

        return View(payment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var payment = await _bll.FindPaymentAsync(id);
        if (payment == null) return NotFound();

        await _bll.DeletePaymentAsync(payment.Id);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateOrderOptionsAsync(PaymentCreateViewModel vm, Guid? selectedOrderId = null)
    {
        vm.OrderOptions = await BuildOrderOptionsAsync(selectedOrderId);
    }

    private async Task PopulateOrderOptionsAsync(PaymentEditViewModel vm, Guid? selectedOrderId = null)
    {
        vm.OrderOptions = await BuildOrderOptionsAsync(selectedOrderId);
    }

    private async Task<List<SelectListItem>> BuildOrderOptionsAsync(Guid? selectedOrderId = null)
    {
        return (await _bll.GetOrdersAsync())
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
        var total = (await _bll.GetOrderAsync(orderId))?.TotalPrice;
        if (!total.HasValue) return NotFound();

        return Json(new { totalPrice = total.Value });
    }
}
