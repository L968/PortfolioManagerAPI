using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;

internal sealed class BuyInvestmentProductHandler(
    IInvestmentProductRepository investmentProductRepository,
    ITransactionRepository transactionRepository,
    IUserProductRepository userProductRepository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    ILogger<BuyInvestmentProductHandler> logger
) : IRequestHandler<BuyInvestmentProductCommand, BuyInvestmentProductResponse>
{
    private readonly IInvestmentProductRepository _investmentProductRepository = investmentProductRepository;
    private readonly ITransactionRepository _transactionRepository = transactionRepository;
    private readonly IUserProductRepository _userProductRepository = userProductRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<BuyInvestmentProductHandler> _logger = logger;

    public async Task<BuyInvestmentProductResponse> Handle(BuyInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        var investmentProduct = await _investmentProductRepository.GetByIdAsync(request.InvestmentProductId, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"Investment product with Id {request.InvestmentProductId} not found.");
        }

        var newTransaction = new Transaction
        {
            UserId = request.UserId,
            InvestmentProductId = request.InvestmentProductId,
            InvestmentProductName = investmentProduct.Name,
            Date = DateTime.UtcNow,
            Price = investmentProduct.Price,
            Quantity = request.Quantity,
            Type = TransactionType.Buy
        };

        _transactionRepository.Create(newTransaction);

        var userProduct = await _userProductRepository.GetUserProductAsync(request.UserId, request.InvestmentProductId, cancellationToken);

        if (userProduct is null)
        {
            var newUserProduct = new UserProduct
            {
                UserId = request.UserId,
                InvestmentProductId = request.InvestmentProductId,
                Quantity = request.Quantity,
                AveragePrice = investmentProduct.Price
            };

            _userProductRepository.Create(newUserProduct);
        }
        else
        {
            var totalQuantity = userProduct.Quantity + request.Quantity;
            var totalValue = (userProduct.AveragePrice * userProduct.Quantity) + (investmentProduct.Price * request.Quantity);

            userProduct.Quantity = totalQuantity;
            userProduct.AveragePrice = totalValue / totalQuantity;

            _userProductRepository.Update(userProduct);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        string cacheKey = CacheKeys.InvestmentProductStatement(request.UserId);
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        _logger.LogInformation("User {UserId} successfully purchased {Quantity} units of product {InvestmentProductId}",
            request.UserId, request.Quantity, request.InvestmentProductId);

        return new BuyInvestmentProductResponse
        {
            TransactionId = newTransaction.Id,
            InvestmentProductId = request.InvestmentProductId,
            Quantity = request.Quantity,
            Price = investmentProduct.Price
        };
    }
}
