using System.Text.Json.Serialization;

namespace PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;

public class BuyInvestmentProductCommand : IRequest<BuyInvestmentProductResponse>
{
    [JsonIgnore]
    public int UserId { get; set; }
    public int InvestmentProductId { get; set; }
    public int Quantity { get; set; }
}
