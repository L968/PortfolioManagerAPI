using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;

internal sealed class DeleteInvestmentProductHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<DeleteInvestmentProductHandler> logger
    ) : IRequestHandler<DeleteInvestmentProductCommand>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<DeleteInvestmentProductHandler> _logger = logger;

    public async Task Handle(DeleteInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        InvestmentProduct? investmentProduct = await _context.InvestmentProducts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"No Investment Product found with Id {request.Id}");
        }

        _context.InvestmentProducts.Remove(investmentProduct);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.InvestmentProducts, cancellationToken);

        _logger.LogInformation("Successfully deleted Investment Product with Id {Id}", request.Id);
    }
}
