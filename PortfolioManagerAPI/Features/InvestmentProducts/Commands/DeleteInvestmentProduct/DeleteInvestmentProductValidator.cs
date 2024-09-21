namespace PortfolioManagerAPI.Features.InvestmentProducts.Commands.DeleteInvestmentProduct;

public class DeleteInvestmentProductValidator : AbstractValidator<DeleteInvestmentProductCommand>
{
    public DeleteInvestmentProductValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0);
    }
}
