namespace Attendify.Common.Persistence;

public static class ActiveUserLock
{
    public static async Task<bool> AcquireAsync(
        ApplicationDbContext dbContext,
        int userId,
        CancellationToken cancellationToken
    )
    {
        if (dbContext.Database.CurrentTransaction is null)
            throw new InvalidOperationException("Active user updates require a transaction.");

        // A conditional EF update locks the user row until the caller's transaction commits.
        int updated = await dbContext.Users
            .Where(user => user.Id == userId && !user.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(user => user.AttendanceEnabled, user => user.AttendanceEnabled),
                cancellationToken
            );
        return updated == 1;
    }
}
