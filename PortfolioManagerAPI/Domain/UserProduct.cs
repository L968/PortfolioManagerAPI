namespace PortfolioManagerAPI.Domain;

public class UserProduct
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int InvestmentProductId { get; set; }
    public int Quantity { get; set; }
    public decimal AveragePrice { get; set; }

    public InvestmentProduct InvestmentProduct { get; set; } = null!;
}
