namespace PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
