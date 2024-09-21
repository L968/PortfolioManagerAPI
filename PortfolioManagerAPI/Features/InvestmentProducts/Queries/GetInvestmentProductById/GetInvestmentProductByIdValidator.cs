namespace PortfolioManagerAPI.Features.InvestmentProducts.Queries.GetInvestmentProductById;

public class GetInvestmentProductByIdValidator : AbstractValidator<GetInvestmentProductByIdQuery>
{
    public GetInvestmentProductByIdValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0);
    }
}
