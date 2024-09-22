using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.CreateInvestmentProduct;

internal sealed class CreateInvestmentProductHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<CreateInvestmentProductHandler> logger
    ) : IRequestHandler<CreateInvestmentProductCommand, CreateInvestmentProductResponse>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<CreateInvestmentProductHandler> _logger = logger;

    public async Task<CreateInvestmentProductResponse> Handle(CreateInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        InvestmentProduct? existingInvestmentProduct = await _context.InvestmentProducts
            .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

        if (existingInvestmentProduct is not null)
        {
            throw new AppException($"A Investment Product with the name \"{request.Name}\" already exists");
        }

        var investmentProduct = new InvestmentProduct
        {
            Name = request.Name,
            Type = request.Type,
            Price = request.Price,
            ExpirationDate = request.ExpirationDate,
        };

        _context.InvestmentProducts.Add(investmentProduct);
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.InvestmentProducts, cancellationToken);

        _logger.LogInformation("Successfully create {@InvestmentProduct}", investmentProduct);

        return new CreateInvestmentProductResponse
        {
            Id = investmentProduct.Id,
            Name = investmentProduct.Name,
            Type = investmentProduct.Type,
            Price = investmentProduct.Price,
            ExpirationDate = investmentProduct.ExpirationDate
        };
    }
}
