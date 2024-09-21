using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;

internal sealed class GetInvestmentProductSummaryHandler(
    AppDbContext context,
    ILogger<GetInvestmentProductSummaryHandler> logger
    ) : IRequestHandler<GetInvestmentProductSummaryQuery, List<GetInvestmentProductSummaryResponse>>
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<GetInvestmentProductSummaryHandler> _logger = logger;

    public async Task<List<GetInvestmentProductSummaryResponse>> Handle(GetInvestmentProductSummaryQuery request, CancellationToken cancellationToken)
    {
        var userProducts = await _context.UserProducts
            .Where(up => up.UserId == request.UserId)
            .Include(up => up.InvestmentProduct)
            .ToListAsync(cancellationToken);

        if (userProducts.Count == 0)
        {
            _logger.LogInformation("No products found for user {UserId}", request.UserId);
            return [];
        }

        var response = new List<GetInvestmentProductSummaryResponse>();

        foreach (var userProduct in userProducts)
        {
            response.Add(new GetInvestmentProductSummaryResponse
            {
                InvestmentProductId = userProduct.InvestmentProduct.Id,
                Name = userProduct.InvestmentProduct.Name,
                Type = userProduct.InvestmentProduct.Type,
                Quantity = userProduct.Quantity,
                AveragePrice = userProduct.AveragePrice,
                CurrentPrice = userProduct.InvestmentProduct.Price
            });
        }

        _logger.LogInformation("Retrieved investment statement for user {UserId}", request.UserId);

        return response;
    }
}
