using PortfolioManagerAPI.Entities;
using System.Text.Json.Serialization;

namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;

public class UpdateInvestmentProductCommand : IRequest
{
    [JsonIgnore]
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public InvestmentProductType Type { get; set; }
    public decimal Price { get; set; }
    public DateTime ExpirationDate { get; set; }
}
