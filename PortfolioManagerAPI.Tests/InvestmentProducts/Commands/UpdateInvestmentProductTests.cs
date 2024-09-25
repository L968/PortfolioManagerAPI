using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Commands;

public class UpdateInvestmentProductTests
{
    private readonly Mock<IInvestmentProductRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<UpdateInvestmentProductHandler>> _loggerMock;
    private readonly UpdateInvestmentProductHandler _handler;

    public UpdateInvestmentProductTests()
    {
        _repositoryMock = new Mock<IInvestmentProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<UpdateInvestmentProductHandler>>();

        _handler = new UpdateInvestmentProductHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldUpdateInvestmentProduct_WhenProductExists()
    {
        // Arrange
        var existingProduct = new InvestmentProduct
        {
            Id = 1,
            Name = "Old Product",
            Type = InvestmentProductType.Stock,
            Price = 100m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        var command = new UpdateInvestmentProductCommand
        {
            Id = 1,
            Name = "Updated Product",
            Type = InvestmentProductType.Fund,
            Price = 150m,
            ExpirationDate = new DateTime(2026, 1, 1)
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(existingProduct);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.Update(existingProduct), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync(CacheKeys.InvestmentProducts, It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal("Updated Product", existingProduct.Name);
        Assert.Equal(InvestmentProductType.Fund, existingProduct.Type);
        Assert.Equal(150m, existingProduct.Price);
        Assert.Equal(new DateTime(2026, 1, 1), existingProduct.ExpirationDate);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new UpdateInvestmentProductCommand
        {
            Id = 999,
            Name = "Nonexistent Product",
            Type = InvestmentProductType.Stock,
            Price = 100m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((InvestmentProduct?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"No Investment Product found with Id {command.Id}", exception.Message);

        _repositoryMock.Verify(x => x.Update(It.IsAny<InvestmentProduct>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
