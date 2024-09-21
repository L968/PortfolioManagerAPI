namespace PortfolioManagerAPI.Entities;

public class InvestmentProduct
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpirationDate { get; set; }

    public List<Transaction> Transactions { get; set; } = [];
    public List<UserProduct> UserProducts { get; set; } = [];
}
