using Attendify.Common.Interfaces;
using Attendify.Common.Authentication;

namespace MigrationService;

public class MigrationUserService : ICurrentUserService
{
    public string? UserId => "MigrationService";
    public UserType? UserType => null;
    public string? StudentId => null;
}