using Attendify.Common.Domain.Authentication;

namespace Attendify.Features.Auth.PasswordReset;

internal sealed record PasswordResetContext(int UserId, PasswordResetToken? Token);
