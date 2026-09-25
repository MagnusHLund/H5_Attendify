using Attendify.Common.Domain.FacialRecognition;
using Attendify.Common.Domain.Users;
using Attendify.Common.FacialRecognition;
using attendanceClass = Attendify.Common.Domain.Attendance;

namespace Attendify.Features.Attendance.CreateAttendance;


public class CreateAttendanceEndpoint(ApplicationDbContext dbContext, IFacialUserIdentifier facialUserIdentifier)
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

            var attendance = attendanceClass.Attendance.Create(req.Classroom, userId);

            dbContext.Attendances.Add(attendance);
            await dbContext.SaveChangesAsync(ct);

            await Send.CreatedAtAsync<CreateAttendanceEndpoint>(cancellation: ct);

        }
        catch (ArgumentException)
        {
            AddError(FacialError.InvalidBase64.Description);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
        catch (FacePhotoValidationException exception)
        {
            AddError(exception.Message);
            await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        }
        catch (NoMatchingUserException)
        {
            AddError(FacialError.NotFound.Description);
            await Send.ErrorsAsync(StatusCodes.Status404NotFound, ct);
        }
    }
}