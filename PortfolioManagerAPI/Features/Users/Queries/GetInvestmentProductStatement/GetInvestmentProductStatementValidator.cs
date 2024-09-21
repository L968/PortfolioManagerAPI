namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductStatement;

public class GetInvestmentProductStatementValidator : AbstractValidator<GetInvestmentProductStatementQuery>
{
    public GetInvestmentProductStatementValidator()
    {
        RuleFor(p => p.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
