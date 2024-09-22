using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Services;

public interface ITokenService
{
    string GenerateToken(Role role);
}
