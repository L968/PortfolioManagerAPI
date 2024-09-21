namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;

public class GetInvestmentProductStatementQuery : IRequest<List<GetInvestmentProductStatementResponse>>
{
    public int UserId { get; set; }
}
