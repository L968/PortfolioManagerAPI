namespace PortfolioManagerAPI.Infrastructure;

public static class CacheKeys
{
    public const string InvestmentProducts = "InvestmentProducts";
    public static string InvestmentProductStatement(int userId) => $"User_{userId}_InvestmentProductStatements";
}
