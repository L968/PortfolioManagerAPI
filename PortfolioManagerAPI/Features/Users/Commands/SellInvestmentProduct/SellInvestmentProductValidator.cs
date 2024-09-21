namespace PortfolioManagerAPI.Features.Users.Commands.SellInvestmentProduct;

public class SellInvestmentProductValidator : AbstractValidator<SellInvestmentProductCommand>
{
    public SellInvestmentProductValidator()
    {
        RuleFor(p => p.InvestmentProductId)
            .GreaterThan(0);

        RuleFor(p => p.Quantity)
            .GreaterThan(0)
            .WithMessage("The quantity must be greater than zero.");
    }
}
