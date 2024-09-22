using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;

internal sealed class UpdateInvestmentProductHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<UpdateInvestmentProductHandler> logger
    ) : IRequestHandler<UpdateInvestmentProductCommand>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<UpdateInvestmentProductHandler> _logger = logger;

    public async Task Handle(UpdateInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        InvestmentProduct? existingInvestmentProduct = await _context.InvestmentProducts
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (existingInvestmentProduct is null)
        {
            throw new AppException($"No Investment Product found with Id {request.Id}");
        }

        existingInvestmentProduct.Name = request.Name;
        existingInvestmentProduct.Type = request.Type;
        existingInvestmentProduct.Price = request.Price;
        existingInvestmentProduct.ExpirationDate = request.ExpirationDate;

        _context.InvestmentProducts.Update(existingInvestmentProduct);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.InvestmentProducts, cancellationToken);

        _logger.LogInformation("Successfully updated {@InvestmentProduct}", existingInvestmentProduct);
    }
}
