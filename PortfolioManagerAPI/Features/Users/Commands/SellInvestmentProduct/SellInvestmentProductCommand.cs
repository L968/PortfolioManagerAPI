using System.Text.Json.Serialization;

namespace PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;

public class SellInvestmentProductCommand : IRequest<SellInvestmentProductResponse>
{
    [JsonIgnore]
    public int UserId { get; set; }
    public int InvestmentProductId { get; set; }
    public int Quantity { get; set; }
}
