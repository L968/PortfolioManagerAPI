using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProducts;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Queries;

public class GetInvestmentProductsTests
{
    private readonly Mock<IInvestmentProductRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<GetInvestmentProductsHandler>> _loggerMock;
    private readonly GetInvestmentProductsHandler _handler;

    public GetInvestmentProductsTests()
    {
        _repositoryMock = new Mock<IInvestmentProductRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<GetInvestmentProductsHandler>>();

        _handler = new GetInvestmentProductsHandler(_repositoryMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnListOfInvestmentProducts_WhenProductsExist()
    {
        // Arrange
        var investmentProducts = new List<InvestmentProduct>
        {
            new() { Id = 1, Name = "Product A", Type = InvestmentProductType.Stock, Price = 100m, ExpirationDate = new DateTime(2025, 1, 1) },
            new() { Id = 2, Name = "Product B", Type = InvestmentProductType.Fund, Price = 200m, ExpirationDate = new DateTime(2026, 1, 1) },
        };

        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(investmentProducts);
        var query = new GetInvestmentProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Product A", result[0].Name);
        Assert.Equal("Product B", result[1].Name);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoProductsExist()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var query = new GetInvestmentProductsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}