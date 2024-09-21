namespace PortfolioManagerAPI.Features.Users.Commands.BuyInvestmentProduct;

public class BuyInvestmentProductValidator : AbstractValidator<BuyInvestmentProductCommand>
{
    public BuyInvestmentProductValidator()
    {
        RuleFor(p => p.InvestmentProductId)
            .GreaterThan(0);

        RuleFor(p => p.Quantity)
            .GreaterThan(0)
            .WithMessage("The quantity must be greater than zero.");
    }
}
