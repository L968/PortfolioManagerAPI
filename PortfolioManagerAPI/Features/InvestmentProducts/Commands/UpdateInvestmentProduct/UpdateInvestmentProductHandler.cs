using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;

internal sealed class UpdateInvestmentProductHandler(
    IInvestmentProductRepository repository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    ILogger<UpdateInvestmentProductHandler> logger
    ) : IRequestHandler<UpdateInvestmentProductCommand>
{
    private readonly IInvestmentProductRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<UpdateInvestmentProductHandler> _logger = logger;

    public async Task Handle(UpdateInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        InvestmentProduct? investmentProduct = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"No Investment Product found with Id {request.Id}");
        }

        investmentProduct.Name = request.Name;
        investmentProduct.Type = request.Type;
        investmentProduct.Price = request.Price;
        investmentProduct.ExpirationDate = request.ExpirationDate;

        _repository.Update(investmentProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.InvestmentProducts, cancellationToken);

        _logger.LogInformation("Successfully updated {@InvestmentProduct}", investmentProduct);
    }
}
