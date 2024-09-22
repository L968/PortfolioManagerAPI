using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Services;

public interface IEmailService
{
    Task SendExpiringProductsEmail(List<InvestmentProduct> products);
}
