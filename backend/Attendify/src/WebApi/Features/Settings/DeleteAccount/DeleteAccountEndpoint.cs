using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Services;
using Attendify.Common.Persistence;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Attendify.Features.Settings.DeleteAccount;

public sealed class DeleteAccountEndpoint(
    ApplicationDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IStudentIdProtector studentIdProtector,
    IAuthenticationCookieService cookieService,
    IServiceScopeFactory scopeFactory
) : Endpoint<DeleteAccountRequest>
{
    public override void Configure()
    {
        Delete("/account");
        Group<SettingsGroup>();
        Policies(JwtOptions.StudentPolicy);
        Description(x => x.WithName("DeleteAccount"));
    }

    public override async Task HandleAsync(DeleteAccountRequest request, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(AttendifyClaimTypes.UserId), out int userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        DeleteResult result = await strategy.ExecuteAsync(async () =>
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            ApplicationDbContext attempt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await using var transaction = await attempt.Database.BeginTransactionAsync(ct);

            if (!await ActiveUserLock.AcquireAsync(attempt, userId, ct))
                return DeleteResult.Unauthorized;
            User user = await attempt.Users.SingleAsync(candidate => candidate.Id == userId, ct);

            if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword)
                == PasswordVerificationResult.Failed)
                return DeleteResult.InvalidPassword;

            await attempt.Entry(user).Reference(candidate => candidate.FacialProfile).LoadAsync(ct);
            if (user.FacialProfile is not null)
            {
                await attempt.Entry(user.FacialProfile)
                    .Collection(profile => profile.FacialEmbeddings).LoadAsync(ct);
                attempt.FacialEmbeddings.RemoveRange(user.FacialProfile.FacialEmbeddings);
                attempt.FacialProfiles.Remove(user.FacialProfile);
            }

            user.AnonymizeForDeletion(
                $"deleted-{user.Id}-{Guid.NewGuid():N}@deleted.invalid",
                studentIdProtector.Protect(Guid.NewGuid().ToString("N")),
                DateTimeOffset.UtcNow
            );
            user.UpdatePassword(passwordHasher.HashPassword(user, Guid.NewGuid().ToString("N")));

            await attempt.AttendanceDetections.Where(item => item.UserId == userId).ExecuteDeleteAsync(ct);
            await attempt.AttendanceRecords.Where(item => item.UserId == userId).ExecuteDeleteAsync(ct);
            UserId attendanceUserId = UserId.From(userId);
            await attempt.Attendances.Where(item => item.UserId == attendanceUserId).ExecuteDeleteAsync(ct);
            await attempt.StudentAccessCodes.Where(item => item.UserId == userId).ExecuteDeleteAsync(ct);
            await attempt.PasswordResetTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(ct);
            await attempt.RefreshTokens.Where(item => item.UserId == userId).ExecuteDeleteAsync(ct);

            await attempt.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return DeleteResult.Deleted;
        });

        if (result == DeleteResult.InvalidPassword)
        {
            AddError("The current password is incorrect.");
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
            return;
        }

        cookieService.ClearAuthenticationCookies();
        if (result == DeleteResult.Unauthorized)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        await Send.NoContentAsync(ct);
    }

    private enum DeleteResult { Deleted, Unauthorized, InvalidPassword }
}
