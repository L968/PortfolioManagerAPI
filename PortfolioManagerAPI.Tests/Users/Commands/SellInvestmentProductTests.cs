using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.Users.Commands;

public class SellInvestmentProductTests
{
    private readonly Mock<IInvestmentProductRepository> _investmentProductRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IUserProductRepository> _userProductRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<SellInvestmentProductHandler>> _loggerMock;
    private readonly SellInvestmentProductHandler _handler;

    public SellInvestmentProductTests()
    {
        _investmentProductRepositoryMock = new Mock<IInvestmentProductRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _userProductRepositoryMock = new Mock<IUserProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<SellInvestmentProductHandler>>();

        _handler = new SellInvestmentProductHandler(
            _investmentProductRepositoryMock.Object,
            _transactionRepositoryMock.Object,
            _userProductRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheMock.Object,
            _loggerMock.Object
        );
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

        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InvestmentProduct?)null);

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
        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(investmentProduct);
        _userProductRepositoryMock.Setup(r => r.GetUserProductAsync(command.UserId, command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProduct?)null);

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

        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(investmentProduct);
        _userProductRepositoryMock.Setup(r => r.GetUserProductAsync(command.UserId, command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProduct);

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

        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(investmentProduct);
        _userProductRepositoryMock.Setup(r => r.GetUserProductAsync(command.UserId, command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Quantity);
        Assert.Equal(100m, result.Price);

        _transactionRepositoryMock.Verify(r => r.Create(It.IsAny<Transaction>()), Times.Once);

        Assert.Equal(3, userProduct.Quantity);
        _userProductRepositoryMock.Verify(r => r.Update(userProduct), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
