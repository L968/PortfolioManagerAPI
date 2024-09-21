namespace PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;

public record SellInvestmentProductResponse
{
    public int TransactionId { get; set; }
    public int InvestmentProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

