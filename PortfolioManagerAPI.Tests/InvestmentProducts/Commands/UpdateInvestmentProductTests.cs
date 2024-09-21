using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Commands;

public class UpdateInvestmentProductTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<UpdateInvestmentProductHandler>> _loggerMock;
    private readonly UpdateInvestmentProductHandler _handler;

    public UpdateInvestmentProductTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<UpdateInvestmentProductHandler>>();

        _handler = new UpdateInvestmentProductHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
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

        _contextMock.Setup(x => x.InvestmentProducts)
            .ReturnsDbSet([existingProduct]);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(x => x.InvestmentProducts.Update(It.IsAny<InvestmentProduct>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet([]);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"No Investment Product found with Id {command.Id}", exception.Message);

        _contextMock.Verify(x => x.InvestmentProducts.Update(It.IsAny<InvestmentProduct>()), Times.Never);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
