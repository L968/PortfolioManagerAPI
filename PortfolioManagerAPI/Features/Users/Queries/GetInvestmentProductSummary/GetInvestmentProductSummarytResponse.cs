using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;

public record GetInvestmentProductSummaryResponse
{
    public int InvestmentProductId { get; set; }
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public int Quantity { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal CurrentPrice { get; set; }
}
