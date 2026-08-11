using Base.Domain;
using MediatR;
using Modules.Orders.Application.Services;
using Modules.Orders.Domain;
using Modules.Orders.Infrastructure;
using Moq;
using Shared.Contracts.Identity;
using Shared.Contracts.Orders;

namespace WebApp.Tests.Unit.BLL.Active;

public class PaymentMapping_Active_Tests
{
    [Fact]
    public void Add_MapsDtoToDomain_AllFieldsCopied()
    {
        var repo = new Mock<IPaymentRepository>();
        var mediator = new Mock<IMediator>();
        var sut = new PaymentService(repo.Object, mediator.Object);
        var dto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            Amount = 10m,
            PaymentStatus = "Paid",
            PaymentMethod = "Card",
            PaidAt = DateTime.UtcNow,
            Summary = "summary"
        };

        sut.Add(dto);

        repo.Verify(r => r.Add(It.Is<Payment>(x =>
            x.Id == dto.Id &&
            x.OrderId == dto.OrderId &&
            x.Amount == dto.Amount &&
            x.PaymentStatus == dto.PaymentStatus &&
            x.PaymentMethod == dto.PaymentMethod &&
            x.Summary!.ToString() == dto.Summary
        )), Times.Once);
    }

    [Fact]
    public async Task FindAsync_MapsDomainToDto_AllFieldsCopied()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var domain = new Payment
        {
            Id = id,
            OrderId = Guid.NewGuid(),
            Amount = 10m,
            PaymentStatus = "Paid",
            PaymentMethod = "Card",
            PaidAt = DateTime.UtcNow,
            Summary = new LangStr("summary"),
            Order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            }
        };

        var repo = new Mock<IPaymentRepository>();
        var mediator = new Mock<IMediator>();
        repo.Setup(r => r.FindByIdAsync(id, Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);
        mediator.Setup(m => m.Send(It.IsAny<GetUserDisplayNameQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserDisplayNameResponse(userId, "u@test"));

        var sut = new PaymentService(repo.Object, mediator.Object);
        var dto = await sut.FindAsync(id);

        Assert.NotNull(dto);
        Assert.Equal(id, dto!.Id);
        Assert.Equal(domain.OrderId, dto.OrderId);
        Assert.Equal(10m, dto.Amount);
        Assert.Equal("Paid", dto.PaymentStatus);
        Assert.Equal("Card", dto.PaymentMethod);
        Assert.Equal("summary", dto.Summary);
    }
}
