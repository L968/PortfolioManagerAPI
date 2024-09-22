using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.CreateInvestmentProduct;

public class CreateInvestmentProductCommand : IRequest<CreateInvestmentProductResponse>
{
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpirationDate { get; set; }
}
