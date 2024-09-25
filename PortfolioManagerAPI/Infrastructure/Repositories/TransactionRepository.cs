using PortfolioManagerAPI.Domain;
using PortfolioManagerAPI.Infrastructure.Repositories.Interfaces;

namespace PortfolioManagerAPI.Infrastructure.Repositories;

public class TransactionRepository(AppDbContext context) : ITransactionRepository
{
    private readonly AppDbContext _context = context;

    public Transaction Create(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        return transaction;
    }
}
