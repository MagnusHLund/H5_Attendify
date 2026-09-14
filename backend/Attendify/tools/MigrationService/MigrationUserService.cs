using Attendify.Common.Interfaces;

namespace MigrationService;

public class MigrationUserService : ICurrentUserService
{
    public string? UserId => "MigrationService";
}