using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProducts;
using PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.Users.Commands;

public class BuyInvestmentProductTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<BuyInvestmentProductHandler>> _loggerMock;
    private readonly BuyInvestmentProductHandler _handler;

    public BuyInvestmentProductTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<BuyInvestmentProductHandler>>();
        _handler = new BuyInvestmentProductHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenInvestmentProductNotFound()
    {
        // Arrange
        var command = new BuyInvestmentProductCommand
        {
            UserId = 1,
            InvestmentProductId = 999,
            Quantity = 1
        };

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Investment product with Id {command.InvestmentProductId} not found.", exception.Message);
    }

    [Fact]
    public async Task ShouldAddNewUserProduct_WhenUserDoesNotOwnProduct()
    {
        // Arrange
        var command = new BuyInvestmentProductCommand
        {
            UserId = 1,
            InvestmentProductId = 1,
            Quantity = 10
        };

        var investmentProduct = new InvestmentProduct { Id = 1, Price = 100m };

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([investmentProduct]);
        _contextMock.Setup(c => c.UserProducts).ReturnsDbSet([]);
        _contextMock.Setup(c => c.Transactions).ReturnsDbSet([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Quantity);
        Assert.Equal(100m, result.Price);

        _contextMock.Verify(c => c.Transactions.Add(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task ShouldUpdateExistingUserProduct_WhenUserAlreadyOwnsProduct()
    {
        // Arrange
        var command = new BuyInvestmentProductCommand
        {
            UserId = 1,
            InvestmentProductId = 1,
            Quantity = 5
        };

        var investmentProduct = new InvestmentProduct { Id = 1, Price = 100m };
        var userProduct = new UserProduct { UserId = 1, InvestmentProductId = 1, Quantity = 10, AveragePrice = 80m };

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([investmentProduct]);
        _contextMock.Setup(c => c.UserProducts).ReturnsDbSet([userProduct]);
        _contextMock.Setup(c => c.Transactions).ReturnsDbSet([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
        Assert.Equal(100m, result.Price);

        _contextMock.Verify(c => c.Transactions.Add(It.IsAny<Transaction>()), Times.Once);

        Assert.Equal(15, userProduct.Quantity);
        Assert.Equal(86.67m, Math.Round(userProduct.AveragePrice, 2));
    }
}
