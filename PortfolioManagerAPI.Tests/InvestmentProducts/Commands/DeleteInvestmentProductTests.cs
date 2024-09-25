using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Commands;

public class DeleteInvestmentProductTests
{
    private readonly Mock<IInvestmentProductRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<DeleteInvestmentProductHandler>> _loggerMock;
    private readonly DeleteInvestmentProductHandler _handler;

    public DeleteInvestmentProductTests()
    {
        _repositoryMock = new Mock<IInvestmentProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<DeleteInvestmentProductHandler>>();

        _handler = new DeleteInvestmentProductHandler(_repositoryMock.Object, _unitOfWorkMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldDeleteInvestmentProduct_WhenProductExists()
    {
        // Arrange
        var existingProduct = new InvestmentProduct
        {
            Id = 1,
            Name = "Product to Delete",
            Type = InvestmentProductType.Stock,
            Price = 100m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        var command = new DeleteInvestmentProductCommand
        {
            Id = 1
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(existingProduct);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(x => x.Delete(existingProduct), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(x => x.RemoveAsync(CacheKeys.InvestmentProducts, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new DeleteInvestmentProductCommand
        {
            Id = 999
        };

        _repositoryMock.Setup(x => x.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((InvestmentProduct?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"No Investment Product found with Id {command.Id}", exception.Message);

        _repositoryMock.Verify(x => x.Delete(It.IsAny<InvestmentProduct>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
