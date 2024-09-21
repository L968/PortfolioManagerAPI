using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.CreateInvestmentProduct;

public record CreateInvestmentProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpirationDate { get; set; }
}
