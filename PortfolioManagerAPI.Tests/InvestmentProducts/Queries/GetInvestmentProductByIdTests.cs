using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Exceptions;
using PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.InvestmentProducts.Queries;

public class GetInvestmentProductByIdTests
{
    private readonly Mock<ILogger<GetInvestmentProductByIdHandler>> _loggerMock;
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<DbSet<InvestmentProduct>> _dbSetMock;
    private readonly GetInvestmentProductByIdHandler _handler;

    public GetInvestmentProductByIdTests()
    {
        _loggerMock = new Mock<ILogger<GetInvestmentProductByIdHandler>>();

        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _dbSetMock = new Mock<DbSet<InvestmentProduct>>();

        var investmentProduct = new InvestmentProduct
        {
            Id = 1,
            Name = "Test Product",
            Type = InvestmentProductType.Stock,
            Price = 100m,
            ExpirationDate = new DateTime(2025, 1, 1)
        };

        var data = new List<InvestmentProduct> { investmentProduct }.AsQueryable();
        _dbSetMock.As<IQueryable<InvestmentProduct>>().Setup(m => m.Provider).Returns(data.Provider);
        _dbSetMock.As<IQueryable<InvestmentProduct>>().Setup(m => m.Expression).Returns(data.Expression);
        _dbSetMock.As<IQueryable<InvestmentProduct>>().Setup(m => m.ElementType).Returns(data.ElementType);
        _dbSetMock.As<IQueryable<InvestmentProduct>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        _contextMock.Setup(c => c.InvestmentProducts).ReturnsDbSet(_dbSetMock.Object);

        _handler = new GetInvestmentProductByIdHandler(_contextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnInvestmentProduct_WhenProductExists()
    {
        // Arrange
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<AppException>(() => _handler.Handle(query, CancellationToken.None));
        Assert.Equal("Investment Product with Id 999 not found", exception.Message);
    }
}
