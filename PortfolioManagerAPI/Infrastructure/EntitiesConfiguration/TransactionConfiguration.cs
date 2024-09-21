using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Infrastructure.EntitiesConfiguration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder
            .HasOne(t => t.InvestmentProduct)
            .WithMany(p => p.Transactions)
            .HasForeignKey(t => t.InvestmentProductId);
    }
}
