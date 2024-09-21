using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Commands;

public class DeleteInvestmentProductTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<DeleteInvestmentProductHandler>> _loggerMock;
    private readonly DeleteInvestmentProductHandler _handler;

    public DeleteInvestmentProductTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<DeleteInvestmentProductHandler>>();

        _handler = new DeleteInvestmentProductHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
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

        _contextMock.Setup(x => x.InvestmentProducts)
            .ReturnsDbSet(new List<InvestmentProduct> { existingProduct });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.InvestmentProducts.Remove(It.IsAny<InvestmentProduct>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new DeleteInvestmentProductCommand
        {
            Id = 999
        };

        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet(new List<InvestmentProduct>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"No Investment Product found with Id {command.Id}", exception.Message);

        _contextMock.Verify(x => x.InvestmentProducts.Remove(It.IsAny<InvestmentProduct>()), Times.Never);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
