using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Infrastructure;
using System.Text.Json;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProducts;

internal sealed class GetInvestmentProductsHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<GetInvestmentProductsHandler> logger
    ) : IRequestHandler<GetInvestmentProductsQuery, List<GetInvestmentProductsResponse>>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<GetInvestmentProductsHandler> _logger = logger;

    public async Task<List<GetInvestmentProductsResponse>> Handle(GetInvestmentProductsQuery request, CancellationToken cancellationToken)
    {
        string? cachedInvestmentProducts = await _cache.GetStringAsync(CacheKeys.InvestmentProducts, cancellationToken);

        if (!string.IsNullOrEmpty(cachedInvestmentProducts))
        {
            _logger.LogInformation("Retrieved investment products from cache");
            return JsonSerializer.Deserialize<List<GetInvestmentProductsResponse>>(cachedInvestmentProducts) ?? [];
        }

        var investmentProducts = await _context.InvestmentProducts
            .Select(p => new GetInvestmentProductsResponse
            {
                Id = p.Id,
                Name = p.Name,
                Type = p.Type,
                Price = p.Price,
                ExpirationDate = p.ExpirationDate
            })
            .ToListAsync(cancellationToken);

        await _cache.SetStringAsync(
            CacheKeys.InvestmentProducts,
            JsonSerializer.Serialize(investmentProducts),
            cancellationToken
        );

        _logger.LogInformation("Successfully retrieved {Count} investment products", investmentProducts.Count);

        return investmentProducts;
    }
}
