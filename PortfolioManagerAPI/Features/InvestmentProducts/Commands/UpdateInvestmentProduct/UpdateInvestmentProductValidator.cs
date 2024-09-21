namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.UpdateInvestmentProduct;

public class UpdateInvestmentProductValidator : AbstractValidator<UpdateInvestmentProductCommand>
{
    public UpdateInvestmentProductValidator()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .MinimumLength(3);

        RuleFor(p => p.Type)
            .IsInEnum();

        RuleFor(p => p.Price)
            .GreaterThan(0);

        RuleFor(p => p.ExpirationDate)
            .GreaterThan(DateTime.Now);
    }
}
