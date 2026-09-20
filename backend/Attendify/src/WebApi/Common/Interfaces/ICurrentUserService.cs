using Attendify.Common.Authentication;

namespace Attendify.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    UserType? UserType { get; }
    string? StudentId { get; }
}
