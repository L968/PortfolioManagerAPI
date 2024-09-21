namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;

public class GetInvestmentProductSummaryQuery : IRequest<List<GetInvestmentProductSummaryResponse>>
{
    public int UserId { get; set; }
}
