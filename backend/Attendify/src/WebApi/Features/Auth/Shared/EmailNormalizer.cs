namespace Attendify.Features.Auth.Shared;

internal static class EmailNormalizer
{
    internal static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
