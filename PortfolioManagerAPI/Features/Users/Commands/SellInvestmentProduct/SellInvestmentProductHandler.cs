using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;

internal sealed class SellInvestmentProductHandler(
    IInvestmentProductRepository investmentProductRepository,
    ITransactionRepository transactionRepository,
    IUserProductRepository userProductRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    ILogger<SellInvestmentProductHandler> logger
) : IRequestHandler<SellInvestmentProductCommand, SellInvestmentProductResponse>
{
    private readonly IInvestmentProductRepository _investmentProductRepository = investmentProductRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUserProductRepository _userProductRepository = userProductRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<SellInvestmentProductHandler> _logger = logger;

    public async Task<SellInvestmentProductResponse> Handle(SellInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        var investmentProduct = await _investmentProductRepository.GetByIdAsync(request.InvestmentProductId, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"Investment product with Id {request.InvestmentProductId} not found.");
        }

        var userProduct = await _userProductRepository.GetUserProductAsync(request.UserId, request.InvestmentProductId, cancellationToken);

        if (userProduct is null)
        {
            throw new AppException($"User {request.UserId} does not own any of product {request.InvestmentProductId}.");
        }

        if (userProduct.Quantity < request.Quantity)
        {
            throw new AppException($"User {request.UserId} does not have enough quantity to sell. Available: {userProduct.Quantity}, Requested: {request.Quantity}.");
        }

        var newTransaction = new Transaction
        {
            UserId = request.UserId,
            InvestmentProductId = request.InvestmentProductId,
            InvestmentProductName = investmentProduct.Name,
            Date = DateTime.UtcNow,
            Price = investmentProduct.Price,
            Quantity = request.Quantity,
            Type = TransactionType.Sell
        };

        _transactionRepository.Create(newTransaction);

        userProduct.Quantity -= request.Quantity;

        if (userProduct.Quantity == 0)
        {
            _userProductRepository.Delete(userProduct);
        }
        else
        {
            _userProductRepository.Update(userProduct);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string cacheKey = CacheKeys.InvestmentProductStatement(request.UserId);
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("User {UserId} successfully sold {Quantity} units of product {InvestmentProductId}",
            request.UserId, request.Quantity, request.InvestmentProductId);

        return new SellInvestmentProductResponse
        {
            TransactionId = newTransaction.Id,
            InvestmentProductId = request.InvestmentProductId,
            Quantity = request.Quantity,
            Price = investmentProduct.Price
        };
    }
}
