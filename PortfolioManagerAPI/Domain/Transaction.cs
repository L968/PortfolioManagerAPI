namespace PortfolioManagerAPI.Domain;

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int InvestmentProductId { get; set; }
    public string InvestmentProductName { get; set; } = "";
    public DateTime Date { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public TransactionType Type { get; set; }
}
