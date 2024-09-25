using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Infrastructure.Repositories;

public class UserProductRepository(AppDbContext context) : IUserProductRepository
{
    private readonly AppDbContext _context = context;

    public async Task<UserProduct?> GetUserProductAsync(int userId, int investmentProductId, CancellationToken cancellationToken)
    {
        return await _context.UserProducts
            .FirstOrDefaultAsync(up => up.UserId == userId && up.InvestmentProductId == investmentProductId, cancellationToken);
    }

    public UserProduct Create(UserProduct userProduct)
    {
        _context.UserProducts.Add(userProduct);
        return userProduct;
    }

    public void Update(UserProduct userProduct)
    {
        _context.UserProducts.Update(userProduct);
    }

    public void Delete(UserProduct userProduct)
    {
        _context.UserProducts.Remove(userProduct);
    }
}
