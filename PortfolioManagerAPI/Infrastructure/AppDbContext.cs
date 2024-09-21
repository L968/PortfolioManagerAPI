using PortfolioManagerAPI.Entities;

namespace PortfolioManagerAPI.Infrastructure;

public class AppDbContext : DbContext
{
    public virtual DbSet<InvestmentProduct> InvestmentProducts { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<UserProduct> UserProducts { get; set; }
    public virtual DbSet<User> Users { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<decimal>()
            .HavePrecision(65, 2);
    }
}
