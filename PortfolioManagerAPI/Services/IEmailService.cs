using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Services;

public interface IEmailService
{
    Task SendExpiringProductsEmail(List<InvestmentProduct> products);
}
