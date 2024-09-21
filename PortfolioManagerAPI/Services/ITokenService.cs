using PortfolioManagerAPI.Enums;

namespace PortfolioManagerAPI.Services;

public interface ITokenService
{
    string GenerateToken(Role role);
}
