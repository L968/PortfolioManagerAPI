using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Queries;

public class GetInvestmentProductByIdTests
{
    private readonly Mock<IInvestmentProductRepository> _repositoryMock;
    private readonly Mock<ILogger<GetInvestmentProductByIdHandler>> _loggerMock;
    private readonly GetInvestmentProductByIdHandler _handler;

    public GetInvestmentProductByIdTests()
    {
        _repositoryMock = new Mock<IInvestmentProductRepository>();
        _loggerMock = new Mock<ILogger<GetInvestmentProductByIdHandler>>();

        _handler = new GetInvestmentProductByIdHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnInvestmentProduct_WhenProductExists()
    {
        // Arrange
        var investmentProduct = new InvestmentProduct
        {
            Id = 1,
            Name = "Test Product",
            Type = InvestmentProductType.Stock,
            Price = 100m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(investmentProduct);

        var query = new GetInvestmentProductByIdQuery { Id = 1 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(InvestmentProductType.Stock, result.Type);
        Assert.Equal(100m, result.Price);
        Assert.Equal(new DateTime(2025, 1, 1), result.ExpirationDate);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenProductDoesNotExist()
    {
        // Arrange
        var query = new GetInvestmentProductByIdQuery { Id = 999 };

        _repositoryMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((InvestmentProduct?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(query, CancellationToken.None));
        Assert.Equal("Investment Product with Id 999 not found", exception.Message);
    }
}
