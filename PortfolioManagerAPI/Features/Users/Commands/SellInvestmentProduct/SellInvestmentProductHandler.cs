using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Entities;
using PortfolioManagerAPI.Infrastructure;

namespace PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;

internal sealed class SellInvestmentProductHandler(
    AppDbContext context,
    IDistributedCache cache,
    ILogger<SellInvestmentProductHandler> logger
) : IRequestHandler<SellInvestmentProductCommand, SellInvestmentProductResponse>
{
    private readonly AppDbContext _context = context;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<SellInvestmentProductHandler> _logger = logger;

    public async Task<SellInvestmentProductResponse> Handle(SellInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        var investmentProduct = await _context.InvestmentProducts
            .FirstOrDefaultAsync(p => p.Id == request.InvestmentProductId, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"Investment product with Id {request.InvestmentProductId} not found.");
        }

        var userProduct = await _context.UserProducts
            .FirstOrDefaultAsync(up => up.UserId == request.UserId
                                    && up.InvestmentProductId == request.InvestmentProductId, cancellationToken);

        if (userProduct is null)
        {
            throw new AppException($"User {request.UserId} does not own any of product {request.InvestmentProductId}.");
        }

        if (userProduct.Quantity < request.Quantity)
        {
            throw new AppException($"User {request.UserId} does not have enough quantity to sell. Available: {userProduct.Quantity}, Requested: {request.Quantity}.");
        }

        var transaction = new Transaction
        {
            UserId = request.UserId,
            InvestmentProductId = request.InvestmentProductId,
            Date = DateTime.UtcNow,
            Price = investmentProduct.Price,
            Quantity = request.Quantity,
            Type = TransactionType.Sell
        };

        _context.Transactions.Add(transaction);

        userProduct.Quantity -= request.Quantity;

        if (userProduct.Quantity == 0)
        {
            _context.UserProducts.Remove(userProduct);
        }
        else
        {
            _context.UserProducts.Update(userProduct);
        }

        await _context.SaveChangesAsync(cancellationToken);

        string cacheKey = CacheKeys.InvestmentProductStatement(request.UserId);
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("User {UserId} successfully sold {Quantity} units of product {InvestmentProductId}",
            request.UserId, request.Quantity, request.InvestmentProductId);

        return new SellInvestmentProductResponse
        {
            TransactionId = transaction.Id,
            UserId = request.UserId,
            InvestmentProductId = request.InvestmentProductId,
            Quantity = request.Quantity,
            Price = investmentProduct.Price
        };
    }
}
