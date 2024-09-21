namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;

public class DeleteInvestmentProductCommand : IRequest
{
    public int Id { get; set; }
}
