using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;

internal sealed class GetInvestmentProductByIdHandler(
    IInvestmentProductRepository repository,
    ILogger<GetInvestmentProductByIdHandler> logger
    ) : IRequestHandler<GetInvestmentProductByIdQuery, GetInvestmentProductByIdResponse>
{
    private readonly IInvestmentProductRepository _repository = repository;
    private readonly ILogger<GetInvestmentProductByIdHandler> _logger = logger;

    public async Task<GetInvestmentProductByIdResponse> Handle(GetInvestmentProductByIdQuery request, CancellationToken cancellationToken)
    {
        var investmentProduct = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (investmentProduct is null)
        {
            throw new AppException($"Investment Product with Id {request.Id} not found");
        }

        _logger.LogInformation("Successfully retrieved Investment Product with Id {Id}", request.Id);

        return new GetInvestmentProductByIdResponse
        {
            Id = investmentProduct.Id,
            Name = investmentProduct.Name,
            Type = investmentProduct.Type,
            Price = investmentProduct.Price,
            ExpirationDate = investmentProduct.ExpirationDate
        };
    }
}
