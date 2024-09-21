using Microsoft.IdentityModel.Tokens;
using PortfolioManagerAPI.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PortfolioManagerAPI.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly string _jwtKey = configuration["Jwt:Key"]!;

    public string GenerateToken(Role role)
    {
        var userId = role == Role.regular ? "1" : "2";

        var claims = new[]
        {
            new("id", userId),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: creds,
            expires: DateTime.Now.AddHours(1)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
