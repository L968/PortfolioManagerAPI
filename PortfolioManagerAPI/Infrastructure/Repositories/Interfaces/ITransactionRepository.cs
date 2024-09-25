using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

public interface ITransactionRepository
{
    Transaction Create(Transaction transaction);
}
