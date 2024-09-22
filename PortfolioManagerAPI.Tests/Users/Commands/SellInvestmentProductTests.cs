using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Enums;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.Users.Commands;

public class SellInvestmentProductTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<SellInvestmentProductHandler>> _loggerMock;
    private readonly SellInvestmentProductHandler _handler;

    public SellInvestmentProductTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<SellInvestmentProductHandler>>();
        _handler = new SellInvestmentProductHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenInvestmentProductNotFound()
    {
        // Arrange
        var command = new SellInvestmentProductCommand
        {
            UserId = 1,
            InvestmentProductId = 999,
            Quantity = 1
        };

        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet([]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"Investment product with Id {command.InvestmentProductId} not found.", exception.Message);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenUserDoesNotOwnProduct()
    {
        // Arrange
        var command = new SellInvestmentProductCommand
        {
            InvestmentProductId = 1,
            UserId = 1,
            Quantity = 1
        };

        var investmentProduct = new InvestmentProduct { Id = 1, Price = 100m };
        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([investmentProduct]);
        _contextMock.Setup(c => c.UserProducts).ReturnsDbSet([]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"User {command.UserId} does not own any of product {command.InvestmentProductId}.", exception.Message);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenUserDoesNotHaveEnoughQuantity()
    {
        // Arrange
        var command = new SellInvestmentProductCommand
        {
            InvestmentProductId = 1,
            UserId = 1,
            Quantity = 5
        };

        var investmentProduct = new InvestmentProduct { Id = 1, Price = 100m };
        var userProduct = new UserProduct { UserId = 1, InvestmentProductId = 1, Quantity = 3 };

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([investmentProduct]);
        _contextMock.Setup(c => c.UserProducts).ReturnsDbSet([userProduct]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"User {command.UserId} does not have enough quantity to sell. Available: {userProduct.Quantity}, Requested: {command.Quantity}.", exception.Message);
    }

    [Fact]
    public async Task ShouldSellInvestmentProduct_WhenSaleIsSuccessful()
    {
        // Arrange
        var command = new SellInvestmentProductCommand
        {
            InvestmentProductId = 1,
            UserId = 1,
            Quantity = 2
        };

        var investmentProduct = new InvestmentProduct { Id = 1, Price = 100m };
        var userProduct = new UserProduct { UserId = 1, InvestmentProductId = 1, Quantity = 5 };

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet([investmentProduct]);
        _contextMock.Setup(c => c.UserProducts).ReturnsDbSet([userProduct]);
        _contextMock.Setup(c => c.Transactions).ReturnsDbSet([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(100m, result.Price);

        _contextMock.Verify(c => c.Transactions.Add(It.IsAny<Transaction>()), Times.Once);
        Assert.Equal(3, userProduct.Quantity);
    }
}
