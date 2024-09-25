using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

public interface IInvestmentProductRepository
{
    Task<IEnumerable<InvestmentProduct>> GetAllAsync(CancellationToken cancellationToken);
    Task<InvestmentProduct?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<InvestmentProduct?> GetByNameAsync(string name, CancellationToken cancellationToken);
    InvestmentProduct Create(InvestmentProduct investmentProduct);
    void Update(InvestmentProduct investmentProduct);
    void Delete(InvestmentProduct investmentProduct);
}
