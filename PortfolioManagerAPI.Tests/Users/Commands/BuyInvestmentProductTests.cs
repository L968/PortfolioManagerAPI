using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.Users.Commands;

public class BuyInvestmentProductTests
{
    private readonly Mock<IInvestmentProductRepository> _investmentProductRepositoryMock;
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
    private readonly Mock<IUserProductRepository> _userProductRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<BuyInvestmentProductHandler>> _loggerMock;
    private readonly BuyInvestmentProductHandler _handler;

    public BuyInvestmentProductTests()
    {
        _investmentProductRepositoryMock = new Mock<IInvestmentProductRepository>();
        _transactionRepositoryMock = new Mock<ITransactionRepository>();
        _userProductRepositoryMock = new Mock<IUserProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<BuyInvestmentProductHandler>>();

        _handler = new BuyInvestmentProductHandler(
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
        var command = new BuyInvestmentProductCommand
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

        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(investmentProduct);
        _userProductRepositoryMock.Setup(r => r.GetUserProductAsync(command.UserId, command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProduct?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Quantity);
        Assert.Equal(100m, result.Price);

        _transactionRepositoryMock.Verify(r => r.Create(It.IsAny<Transaction>()), Times.Once);
        _userProductRepositoryMock.Verify(r => r.Create(It.IsAny<UserProduct>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _investmentProductRepositoryMock.Setup(r => r.GetByIdAsync(command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(investmentProduct);
        _userProductRepositoryMock.Setup(r => r.GetUserProductAsync(command.UserId, command.InvestmentProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);
        Assert.Equal(100m, result.Price);

        _transactionRepositoryMock.Verify(r => r.Create(It.IsAny<Transaction>()), Times.Once);
        _userProductRepositoryMock.Verify(r => r.Update(userProduct), Times.Once);
        Assert.Equal(15, userProduct.Quantity);
        Assert.Equal(86.67m, Math.Round(userProduct.AveragePrice, 2));
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
