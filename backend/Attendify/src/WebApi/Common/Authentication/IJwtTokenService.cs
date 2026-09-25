using System.Security.Claims;

namespace Attendify.Common.Authentication;

public interface IJwtTokenService
{
    string GenerateToken(IReadOnlyList<Claim> claims);
}
