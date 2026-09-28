using System.Linq.Expressions;
using Attendify.Common.Pagination;

namespace Attendify.Common.Domain.Attendance;

public sealed class AttendanceSpec : Specification<Attendance>
{

    public static SortColumnMap<Attendance> SortColumns { get; } = new(
            defaultColumn: "attendanceDate",
            columns: new Dictionary<string, Expression<Func<Attendance, object?>>>
            {
                ["attendanceDate"] = a => a.AttendanceDate,
                ["arrivedAt"] = a => a.ArrivedAt,
                ["classroom"] = a => a.Classroom,
                ["status"] = a => a.Status
            });

    public static AttendanceSpec ById(AttendanceId attendanceId)
    {
        AttendanceSpec spec = new AttendanceSpec();
        spec.Query.Where(a => a.Id == attendanceId);
        return spec;
    }

    public static AttendanceSpec Paged(PagingParams paging, string? sortBy, SortDirection sortDirection)
    {
        ThrowIfNull(paging);

        AttendanceSpec spec = new AttendanceSpec();
        SortColumns.Apply(spec.Query, sortBy, sortDirection, a => a.Id);
        spec.Query.Skip(paging.Skip).Take(paging.PageSize);
        return spec;
    }
}