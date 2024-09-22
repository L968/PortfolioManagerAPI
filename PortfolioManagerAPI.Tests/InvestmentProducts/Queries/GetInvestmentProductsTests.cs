using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProducts;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Queries;

public class GetInvestmentProductsTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<GetInvestmentProductsHandler>> _loggerMock;
    private readonly GetInvestmentProductsHandler _handler;

    public GetInvestmentProductsTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<GetInvestmentProductsHandler>>();

        var investmentProducts = new List<InvestmentProduct>
        {
            new() { Id = 1, Name = "Product A", Type = InvestmentProductType.Stock, Price = 100m, ExpirationDate = new DateTime(2025, 1, 1) },
            new() { Id = 2, Name = "Product B", Type = InvestmentProductType.Fund, Price = 200m, ExpirationDate = new DateTime(2026, 1, 1) },
        };

        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet(investmentProducts);

        _handler = new GetInvestmentProductsHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnListOfInvestmentProducts_WhenProductsExist()
    {
        // Arrange
        var query = new GetInvestmentProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Product A", result[0].Name);
        Assert.Equal(InvestmentProductType.Stock, result[0].Type);
        Assert.Equal(100m, result[0].Price);
        Assert.Equal(new DateTime(2025, 1, 1), result[0].ExpirationDate);

        Assert.Equal(2, result[1].Id);
        Assert.Equal("Product B", result[1].Name);
        Assert.Equal(InvestmentProductType.Fund, result[1].Type);
        Assert.Equal(200m, result[1].Price);
        Assert.Equal(new DateTime(2026, 1, 1), result[1].ExpirationDate);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet(new List<InvestmentProduct>());
        var query = new GetInvestmentProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
