using MediatR;
using Shared.Contracts.Identity;
using Shared.Contracts.Orders;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;

namespace Modules.Orders.Application.Services;

internal class PaymentService : Modules.Orders.Application.Services.IPaymentService
{
    private readonly IPaymentRepository _payments;
    private readonly IMediator _mediator;

    public PaymentService(IPaymentRepository payments, IMediator mediator)
    {
        _payments = payments;
        _mediator = mediator;
    }

    public async Task<IEnumerable<PaymentDto>> AllAsync(Guid appUserId = default)
    {
        return (await _payments.GetAllAsync(appUserId)).Select(ToDto).ToList();
    }

    public async Task<PaymentDto?> FindAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _payments.FindByIdAsync(id, appUserId);
        return entity == null ? null : ToDto(entity);
    }

    public void Add(PaymentDto entity, Guid appUserId = default)
    {
        _payments.Add(ToDomain(entity));
    }

    public async Task<PaymentDto> UpdateAsync(PaymentDto entity, Guid appUserId = default)
    {
        var dbEntity = await _payments.FindTrackedByIdAsync(entity.Id) ?? throw new InvalidOperationException("Payment not found");
        dbEntity.OrderId = entity.OrderId;
        dbEntity.Amount = entity.Amount;
        dbEntity.PaymentStatus = entity.PaymentStatus;
        dbEntity.PaymentMethod = entity.PaymentMethod;
        dbEntity.PaidAt = NormalizeToUtc(entity.PaidAt);
        dbEntity.Summary = entity.Summary ?? string.Empty;
        return ToDto(dbEntity);
    }

    public void Remove(PaymentDto entity)
    {
        _payments.Remove(ToDomain(entity));
    }

    public async Task RemoveAsync(Guid id, Guid appUserId = default)
    {
        var entity = await _payments.FindForDeleteAsync(id, appUserId);
        if (entity != null) _payments.Remove(entity);
    }

    private PaymentDto ToDto(Payment entity)
    {
        var userDisplayName = entity.Order == null ? null : ResolveUserDisplayName(entity.Order.UserId);
        return new PaymentDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Amount = entity.Amount,
            PaymentStatus = entity.PaymentStatus,
            PaymentMethod = entity.PaymentMethod,
            PaidAt = entity.PaidAt,
            Summary = entity.Summary,
            Order = entity.Order == null
                ? null
                : new PaymentOrderDto
                {
                    Id = entity.Order.Id,
                    UserId = entity.Order.UserId,
                    CreatedAt = entity.Order.CreatedAt,
                    User = new PaymentUserDto
                    {
                        Id = entity.Order.UserId,
                        Email = userDisplayName
                    }
                }
        };
    }

    private static Payment ToDomain(PaymentDto entity)
    {
        return new Payment
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Amount = entity.Amount,
            PaymentStatus = entity.PaymentStatus,
            PaymentMethod = entity.PaymentMethod,
            PaidAt = NormalizeToUtc(entity.PaidAt),
            Summary = entity.Summary ?? string.Empty
        };
    }

    private static DateTime? NormalizeToUtc(DateTime? value)
    {
        if (!value.HasValue) return null;

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value.Value, DateTimeKind.Local).ToUniversalTime(),
            _ => value.Value
        };
    }

    private string ResolveUserDisplayName(Guid userId)
    {
        var response = _mediator.Send(new GetUserDisplayNameQuery(userId)).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(response?.DisplayName) ? userId.ToString() : response.DisplayName;
    }
}
