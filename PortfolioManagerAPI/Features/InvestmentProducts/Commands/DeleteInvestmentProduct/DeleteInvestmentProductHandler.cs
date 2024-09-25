using Microsoft.Extensions.Caching.Distributed;
using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;

internal sealed class DeleteInvestmentProductHandler(
    IInvestmentProductRepository repository,
    IUnitOfWork unitOfWork,
    IDistributedCache cache,
    ILogger<DeleteInvestmentProductHandler> logger
    ) : IRequestHandler<DeleteInvestmentProductCommand>
{
    private readonly IInvestmentProductRepository _repository = repository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<DeleteInvestmentProductHandler> _logger = logger;

    public async Task Handle(DeleteInvestmentProductCommand request, CancellationToken cancellationToken)
    {
        InvestmentProduct? investmentProduct = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"No Investment Product found with Id {request.Id}");
        }

        _repository.Delete(investmentProduct);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(CacheKeys.InvestmentProducts, cancellationToken);

        _logger.LogInformation("Successfully deleted Investment Product with Id {Id}", request.Id);
    }
}
