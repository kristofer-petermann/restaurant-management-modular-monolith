using WebApp.ApiContracts.v1.Mappers;
using AwesomeAssertions;

namespace WebApp.Tests.Unit.API.Mappers;

public class PaymentPublicDTOv1Mapper_Tests
{
    private readonly PaymentPublicDTOv1Mapper _sut = new();

    [Fact]
    public void Map_BllToApi_AllFieldsCopied()
    {
        var orderId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var bll = new Shared.Contracts.Orders.PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 14.20m,
            PaymentStatus = "Paid",
            PaymentMethod = "Card",
            PaidAt = paidAt,
            Summary = "summary"
        };

        var api = _sut.Map(bll)!;

        api.Id.Should().Be(bll.Id);
        api.OrderId.Should().Be(orderId);
        api.Amount.Should().Be(14.20m);
        api.PaymentStatus.Should().Be("Paid");
        api.PaymentMethod.Should().Be("Card");
        api.PaidAt.Should().Be(paidAt);
        api.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_ApiToBll_AllFieldsCopied()
    {
        var orderId = Guid.NewGuid();
        var paidAt = DateTime.UtcNow;
        var api = new WebApp.ApiContracts.v1.PaymentResponse
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 14.20m,
            PaymentStatus = "Paid",
            PaymentMethod = "Card",
            PaidAt = paidAt,
            Summary = "summary"
        };

        var bll = _sut.Map(api)!;

        bll.Id.Should().Be(api.Id);
        bll.OrderId.Should().Be(orderId);
        bll.Amount.Should().Be(14.20m);
        bll.PaymentStatus.Should().Be("Paid");
        bll.PaymentMethod.Should().Be("Card");
        bll.PaidAt.Should().Be(paidAt);
        bll.Summary.Should().Be("summary");
    }

    [Fact]
    public void Map_NullBll_ReturnsNull()
    {
        _sut.Map((Shared.Contracts.Orders.PaymentDto?)null).Should().BeNull();
    }

    [Fact]
    public void Map_NullApi_ReturnsNull()
    {
        _sut.Map((WebApp.ApiContracts.v1.PaymentResponse?)null).Should().BeNull();
    }
}

