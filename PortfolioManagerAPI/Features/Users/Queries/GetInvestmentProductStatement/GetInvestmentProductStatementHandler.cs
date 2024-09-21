using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Infrastructure;
using System.Text.Json;

namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;

internal sealed class GetInvestmentProductStatementHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<GetInvestmentProductStatementHandler> logger
) : IRequestHandler<GetInvestmentProductStatementQuery, List<GetInvestmentProductStatementResponse>>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<GetInvestmentProductStatementHandler> _logger = logger;

    public async Task<List<GetInvestmentProductStatementResponse>> Handle(GetInvestmentProductStatementQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKeys.InvestmentProductStatement(request.UserId);
        string? cachedStatements = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrEmpty(cachedStatements))
        {
            _logger.LogInformation("Retrieved transaction statements from cache for user {UserId}", request.UserId);
            return JsonSerializer.Deserialize<List<GetInvestmentProductStatementResponse>>(cachedStatements) ?? [];
        }

        var transactions = await _context.Transactions
            .Where(t => t.UserId == request.UserId)
            .Include(t => t.InvestmentProduct)
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
        {
            _logger.LogInformation("No products found for user {UserId}", request.UserId);
            return [];
        }

        var response = transactions.Select(transaction => new GetInvestmentProductStatementResponse
        {
            TransactionId = transaction.Id,
            InvestmentProductId = transaction.InvestmentProductId,
            InvestmentProductName = transaction.InvestmentProduct.Name,
            Date = transaction.Date,
            Price = transaction.Price,
            Quantity = transaction.Quantity,
            Type = transaction.Type
        }).ToList();

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(response),
            cancellationToken
        );

        _logger.LogInformation("Retrieved transaction statement for user {UserId}", request.UserId);

        return response;
    }
}
