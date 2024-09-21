using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Infrastructure.EntitiesConfiguration;

public class UserProductConfiguration : IEntityTypeConfiguration<UserProduct>
{
    public void Configure(EntityTypeBuilder<UserProduct> builder)
    {
        builder
            .HasOne(up => up.InvestmentProduct)
            .WithMany(p => p.UserProducts)
            .HasForeignKey(up => up.InvestmentProductId);
    }
}
