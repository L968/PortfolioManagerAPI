namespace PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;

public class GetInvestmentProductByIdQuery : IRequest<GetInvestmentProductByIdResponse>
{
    public int Id { get; set; }
}
