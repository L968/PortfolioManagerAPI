using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Infrastructure.Repositories;

public class InvestmentProductRepository(AppDbContext context) : IInvestmentProductRepository
{
    private readonly AppDbContext _context = context;

    public async Task<IEnumerable<InvestmentProduct>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.InvestmentProducts.ToListAsync(cancellationToken);
    }

    public async Task<InvestmentProduct?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.InvestmentProducts.FindAsync([id], cancellationToken);
    }

    public async Task<InvestmentProduct?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.InvestmentProducts.FindAsync([name], cancellationToken);
    }

    public InvestmentProduct Create(InvestmentProduct investmentProduct)
    {
        _context.InvestmentProducts.Add(investmentProduct);
        return investmentProduct;
    }

    public void Update(InvestmentProduct investmentProduct)
    {
        _context.InvestmentProducts.Update(investmentProduct);
    }

    public void Delete(InvestmentProduct investmentProduct)
    {
        _context.InvestmentProducts.Remove(investmentProduct);
    }
}
