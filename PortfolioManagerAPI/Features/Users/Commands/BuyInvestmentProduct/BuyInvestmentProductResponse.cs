namespace PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;

public record BuyInvestmentProductResponse
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public int InvestmentProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
