using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Services;

public interface IEmailService
{
    Task SendExpiringProductsEmail(List<InvestmentProduct> products);
}
