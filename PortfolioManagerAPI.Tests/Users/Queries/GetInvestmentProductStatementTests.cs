using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Tests.Users.Queries;

public class GetInvestmentProductStatementTests
{
    private readonly Mock<AppDbContext> _contextMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<GetInvestmentProductStatementHandler>> _loggerMock;
    private readonly GetInvestmentProductStatementHandler _handler;

    public GetInvestmentProductStatementTests()
    {
        _contextMock = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<GetInvestmentProductStatementHandler>>();
        _handler = new GetInvestmentProductStatementHandler(_contextMock.Object, _cacheMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ShouldReturnTransactionStatement_WhenTransactionsExist()
    {
        // Arrange
        var userId = 1;
        var transactions = new List<Transaction>
        {
            new() {
                Id = 1,
                UserId = userId,
                InvestmentProductId = 1,
                InvestmentProductName = "Product A",
                Date = DateTime.UtcNow,
                Price = 100m,
                Quantity = 2,
                Type = TransactionType.Buy,
            },
            new() {
                Id = 2,
                UserId = userId,
                InvestmentProductId = 2,
                InvestmentProductName = "Product B",
                Date = DateTime.UtcNow,
                Price = 200m,
                Quantity = 1,
                Type = TransactionType.Sell,
            }
        };

        var query = new GetInvestmentProductStatementQuery { UserId = userId };

        _contextMock.Setup(x => x.Transactions).ReturnsDbSet(transactions);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.TransactionId == 1 && r.InvestmentProductId == 1 && r.Quantity == 2 && r.Price == 100m);
        Assert.Contains(result, r => r.TransactionId == 2 && r.InvestmentProductId == 2 && r.Quantity == 1 && r.Price == 200m);
    }

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoTransactionsExist()
    {
        // Arrange
        var userId = 1;

        _contextMock.Setup(x => x.Transactions).ReturnsDbSet([]);

        var query = new GetInvestmentProductStatementQuery { UserId = userId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
