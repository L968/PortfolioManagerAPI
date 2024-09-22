using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Commands.CreateInvestmentProduct;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Commands;

public class CreateInvestmentProductTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<CreateInvestmentProductHandler>> _loggerMock;
    private readonly CreateInvestmentProductHandler _handler;

    public CreateInvestmentProductTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<CreateInvestmentProductHandler>>();

        _handler = new CreateInvestmentProductHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldCreateNewInvestmentProduct_WhenProductDoesNotExist()
    {
        // Arrange
        var command = new CreateInvestmentProductCommand
        {
            Name = "New Product",
            Type = InvestmentProductType.Stock,
            Price = 150m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        _contextMock.Setup(x => x.InvestmentProducts).ReturnsDbSet([]);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Product", result.Name);
        Assert.Equal(InvestmentProductType.Stock, result.Type);
        Assert.Equal(150m, result.Price);
        Assert.Equal(new DateTime(2025, 1, 1), result.ExpirationDate);
        _contextMock.Verify(x => x.InvestmentProducts.Add(It.IsAny<InvestmentProduct>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrowAppException_WhenProductWithSameNameExists()
    {
        // Arrange
        var existingProduct = new InvestmentProduct
        {
            Name = "Existing Product",
            Type = InvestmentProductType.Fund,
            Price = 200m,
            ExpirationDate = new DateTime(2026, 1, 1)
        };

        var command = new CreateInvestmentProductCommand
        {
            Name = existingProduct.Name,
            Type = InvestmentProductType.Stock,
            Price = 150m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        _contextMock.Setup(x => x.InvestmentProducts)
            .ReturnsDbSet(new List<InvestmentProduct> { existingProduct });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal($"A Investment Product with the name \"{command.Name}\" already exists", exception.Message);

        // Verificar que o método Add e SaveChanges não foram chamados
        _contextMock.Verify(x => x.InvestmentProducts.Add(It.IsAny<InvestmentProduct>()), Times.Never);
        _contextMock.Verify(x => x.SaveChanges(), Times.Never);
    }
}
