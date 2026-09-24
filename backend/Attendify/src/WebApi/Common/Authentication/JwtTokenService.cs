using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Attendify.Common.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(IReadOnlyList<Claim> claims)
    {
        DateTime issuedAt = DateTime.UtcNow;
        DateTime expiresAt = issuedAt.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes);

        byte[] secret = System.Text.Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        SymmetricSecurityKey key = new SymmetricSecurityKey(secret);

        SigningCredentials signingCredentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: issuedAt,
            expires: expiresAt,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
