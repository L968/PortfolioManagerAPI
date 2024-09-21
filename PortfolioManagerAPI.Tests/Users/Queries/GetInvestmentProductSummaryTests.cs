using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.Users.Queries;

public class GetInvestmentProductSummaryTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<ILogger<GetInvestmentProductSummaryHandler>> _loggerMock;
    private readonly GetInvestmentProductSummaryHandler _handler;

    public GetInvestmentProductSummaryTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _loggerMock = new Mock<ILogger<GetInvestmentProductSummaryHandler>>();
        _handler = new GetInvestmentProductSummaryHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnInvestmentProductSummary_WhenProductsExist()
    {
        // Arrange
        var userId = 1;
        var userProducts = new List<UserProduct>
        {
            new() {
                UserId = userId,
                InvestmentProductId = 1,
                Quantity = 10,
                AveragePrice = 90m,
                InvestmentProduct = new() { Id = 1, Name = "Product A", Type = InvestmentProductType.Stock, Price = 100m }
            },
            new() {
                UserId = userId,
                InvestmentProductId = 2,
                Quantity = 5,
                AveragePrice = 200m,
                InvestmentProduct = new() { Id = 2, Name = "Product B", Type = InvestmentProductType.Fund, Price = 250m }
            }
        };

        var query = new GetInvestmentProductSummaryQuery { UserId = userId };

        _contextMock.Setup(x => x.UserProducts).ReturnsDbSet(userProducts);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.InvestmentProductId == 1 && r.Quantity == 10 && r.CurrentPrice == 100m);
        Assert.Contains(result, r => r.InvestmentProductId == 2 && r.Quantity == 5 && r.CurrentPrice == 250m);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        var userId = 1;
        var userProducts = new List<UserProduct>();

        _contextMock.Setup(x => x.UserProducts).ReturnsDbSet(userProducts);

        var query = new GetInvestmentProductSummaryQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
