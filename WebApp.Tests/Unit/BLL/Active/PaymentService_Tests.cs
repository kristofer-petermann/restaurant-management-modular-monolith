using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Orders;

namespace WebApp.Tests.Unit.BLL.Active;

public class PaymentService_Tests
{
    [Fact]
    public async Task UpdateAsync_NormalizesPaidAtToUtc()
    {
        var repo = new Mock<IPaymentRepository>();
        var mediator = new Mock<IMediator>();
        var id = Guid.NewGuid();
        var tracked = new Payment
        {
            Id = id,
            OrderId = Guid.NewGuid(),
            Amount = 5m,
            PaymentStatus = "Pending",
            PaymentMethod = "Card",
            Summary = "s"
        };

        repo.Setup(r => r.FindTrackedByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tracked);

        var localTime = DateTime.SpecifyKind(new DateTime(2026, 5, 21, 10, 0, 0), DateTimeKind.Local);
        var sut = new PaymentService(repo.Object, mediator.Object);
        var result = await sut.UpdateAsync(new PaymentDto
        {
            Id = id,
            OrderId = tracked.OrderId,
            Amount = 12m,
            PaymentStatus = "Paid",
            PaymentMethod = "Card",
            PaidAt = localTime,
            Summary = "done"
        });

        Assert.NotNull(result.PaidAt);
        Assert.Equal(DateTimeKind.Utc, result.PaidAt!.Value.Kind);
        Assert.Equal("Paid", tracked.PaymentStatus);
        Assert.Equal("done", tracked.Summary);
    }

    [Fact]
    public async Task RemoveAsync_RemovesWhenFound()
    {
        var repo = new Mock<IPaymentRepository>();
        var mediator = new Mock<IMediator>();
        var id = Guid.NewGuid();
        var entity = new Payment { Id = id };

        repo.Setup(r => r.FindForDeleteAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var sut = new PaymentService(repo.Object, mediator.Object);
        await sut.RemoveAsync(id);

        repo.Verify(r => r.Remove(entity), Times.Once);
    }
}
