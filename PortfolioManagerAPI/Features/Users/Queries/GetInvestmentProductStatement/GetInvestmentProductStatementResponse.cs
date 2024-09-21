using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;

public record GetInvestmentProductStatementResponse
{
    public int TransactionId { get; set; }
    public int InvestmentProductId { get; set; }
    public string InvestmentProductName { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public TransactionType Type { get; set; }
}
