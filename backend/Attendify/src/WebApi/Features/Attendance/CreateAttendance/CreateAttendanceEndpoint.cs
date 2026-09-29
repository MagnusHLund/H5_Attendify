using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Domain.Users;
using Attendify.Common.FacialRecognition;
using Attendify.Common.Persistence;
using attendanceClass = Attendify.Common.Domain.Attendance;

namespace Attendify.Features.Attendance.CreateAttendance;


public class CreateAttendanceEndpoint(ApplicationDbContext dbContext, IFacialUserIdentifier facialUserIdentifier, IServiceScopeFactory scopeFactory)
    : Endpoint<CreateAttendanceRequest>
{

    public override void Configure()
    {
        Post("/");
        Group<AttendanceGroup>();
        AllowAnonymous();
        Description(x => x.WithName("CreateAttendance"));
    }

    public override async Task HandleAsync(CreateAttendanceRequest req, CancellationToken ct)
    {
        try
        {
            UserId userId = await facialUserIdentifier.IdentifyUserAsync(req.Picture, ct);

            var strategy = dbContext.Database.CreateExecutionStrategy();
            bool recorded = await strategy.ExecuteAsync(async () =>
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                ApplicationDbContext attempt = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await using var transaction = await attempt.Database.BeginTransactionAsync(ct);
                if (!await ActiveUserLock.AcquireAsync(attempt, userId.Value, ct) ||
                    !await attempt.Users.AnyAsync(user => user.Id == userId.Value && user.AttendanceEnabled, ct))
                    return false;

                attempt.Attendances.Add(attendanceClass.Attendance.Create(req.Classroom, userId));
                await attempt.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return true;
            });
            if (!recorded)
                throw new NoMatchingUserException("Recognition is disabled for this account.");

            await Send.CreatedAtAsync<CreateAttendanceEndpoint>(cancellation: ct);

        }
        catch (ArgumentException)
        {
            AddError(FacialErrors.InvalidBase64.Description);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
        catch (FacePhotoValidationException exception)
        {
            AddError(exception.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
        catch (NoMatchingUserException)
        {
            AddError(FacialErrors.NotFound.Description);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}