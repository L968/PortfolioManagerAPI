namespace PortfolioManagerAPI.Features.Users.Queries.GetInvestmentProductSummary;

public class GetInvestmentProductSummaryValidator : AbstractValidator<GetInvestmentProductSummaryQuery>
{
    public GetInvestmentProductSummaryValidator()
    {
        RuleFor(p => p.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than zero.");
    }
}
