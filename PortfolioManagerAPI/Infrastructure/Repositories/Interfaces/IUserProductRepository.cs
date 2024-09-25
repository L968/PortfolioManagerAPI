using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

public interface IUserProductRepository
{
    Task<UserProduct?> GetUserProductAsync(int userId, int investmentProductId, CancellationToken cancellationToken);
    UserProduct Create(UserProduct userProduct);
    void Update(UserProduct userProduct);
    void Delete(UserProduct userProduct);
}
