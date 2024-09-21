namespace PortfolioManagerAPI.Infrastructure;

public static class CacheKeys
{
    public const string InvestmentProducts = "investment_products";
    public static string InvestmentProductStatement(int userId) => $"User_{userId}_InvestmentProductStatements";
}
