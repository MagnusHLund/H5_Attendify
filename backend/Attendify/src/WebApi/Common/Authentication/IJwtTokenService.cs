using System.Security.Claims;

namespace Attendify.Common.Authentication;

public interface IJwtTokenService
{
    Task<string> GenerateTokenAsync(IReadOnlyList<Claim> claims);
}