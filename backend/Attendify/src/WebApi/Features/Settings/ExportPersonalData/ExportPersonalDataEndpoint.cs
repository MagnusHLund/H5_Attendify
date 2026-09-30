using Attendify.Common.Authentication;
using Attendify.Common.Domain.Users;
using Attendify.Common.Services;
using System.Security.Claims;

namespace Attendify.Features.Settings.ExportPersonalData;

public sealed class ExportPersonalDataEndpoint(
    ApplicationDbContext dbContext,
    IStudentIdProtector studentIdProtector
) : EndpointWithoutRequest<ExportPersonalDataResponse>
{
    public override void Configure()
    {
        Get("/personal-data");
        Group<SettingsGroup>();
        Policies(JwtOptions.StudentPolicy);
        Description(x => x.WithName("ExportPersonalData"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirstValue(AttendifyClaimTypes.UserId), out int userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(candidate => candidate.Id == userId && !candidate.IsDeleted)
            .Select(candidate => new
            {
                candidate.Email,
                candidate.EncryptedStudentId,
                candidate.EducationalInstituteId,
                candidate.CreatedAt,
                candidate.AttendanceEnabled,
            })
            .SingleOrDefaultAsync(ct);

        if (user is null)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var attendance = await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(record => record.UserId == userId)
            .OrderBy(record => record.AttendanceDate)
            .Select(record => new ExportPersonalDataResponse.AttendanceData(
                record.Classroom,
                record.AttendanceDate,
                record.ArrivalTime,
                record.DepartureTime,
                record.DepartureKnown
            ))
            .ToListAsync(ct);

        UserId attendanceUserId = UserId.From(userId);
        var attendanceEvents = await dbContext.Attendances
            .AsNoTracking()
            .Where(eventRecord => eventRecord.UserId == attendanceUserId)
            .OrderBy(eventRecord => eventRecord.AttendanceDate)
            .Select(eventRecord => new ExportPersonalDataResponse.AttendanceEventData(
                eventRecord.Classroom,
                eventRecord.AttendanceDate,
                eventRecord.ArrivedAt,
                eventRecord.Status.ToString()
            ))
            .ToListAsync(ct);

        var detections = await dbContext.AttendanceDetections
            .AsNoTracking()
            .Where(detection => detection.UserId == userId)
            .OrderBy(detection => detection.DetectedAt)
            .Select(detection => new ExportPersonalDataResponse.DetectionData(
                detection.Classroom,
                detection.DetectedAt
            ))
            .ToListAsync(ct);

        var accessCodes = await dbContext.StudentAccessCodes
            .AsNoTracking()
            .Where(code => code.UserId == userId)
            .OrderBy(code => code.GenerationDate)
            .Select(code => new ExportPersonalDataResponse.AccessCodeData(
                code.GenerationDate,
                code.ExpiresAt
            ))
            .ToListAsync(ct);

        var response = new ExportPersonalDataResponse(
            DateTimeOffset.UtcNow,
            new ExportPersonalDataResponse.AccountData(
                user.Email,
                studentIdProtector.Unprotect(user.EncryptedStudentId),
                user.EducationalInstituteId,
                user.CreatedAt,
                user.AttendanceEnabled
            ),
            attendanceEvents,
            attendance,
            detections,
            accessCodes
        );

        HttpContext.Response.Headers.ContentDisposition = "attachment; filename=attendify-personal-data.json";
        HttpContext.Response.Headers.CacheControl = "no-store";
        await Send.OkAsync(response, ct);
    }
}
