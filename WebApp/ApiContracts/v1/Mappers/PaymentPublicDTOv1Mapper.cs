using Base.Contracts;
using Shared.Contracts.Orders;

namespace WebApp.ApiContracts.v1.Mappers;

public class PaymentPublicDTOv1Mapper : IBaseMapper<PaymentResponse, PaymentDto>
{
    public PaymentResponse? Map(PaymentDto? entity)
    {
        if (entity == null) return null;
        return new PaymentResponse
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Amount = entity.Amount,
            PaymentStatus = entity.PaymentStatus,
            PaymentMethod = entity.PaymentMethod,
            PaidAt = entity.PaidAt,
            Summary = entity.Summary
        };
    }

    public PaymentDto? Map(PaymentResponse? entity)
    {
        if (entity == null) return null;
        return new PaymentDto
        {
            Id = entity.Id,
            OrderId = entity.OrderId,
            Amount = entity.Amount,
            PaymentStatus = entity.PaymentStatus,
            PaymentMethod = entity.PaymentMethod,
            PaidAt = entity.PaidAt,
            Summary = entity.Summary ?? string.Empty
        };
    }
}
