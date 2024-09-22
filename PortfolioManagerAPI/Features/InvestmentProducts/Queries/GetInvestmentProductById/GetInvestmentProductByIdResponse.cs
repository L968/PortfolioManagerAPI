using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;

public record GetInvestmentProductByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpirationDate { get; set; }
}
