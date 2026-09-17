using Attendify.Common.Domain.Attendance;
namespace Attendify.Common.Persistence;

// INFO: New strongly typed IDs should be registered here

//[EfCoreConverter<TypedID1>]
//[EfCoreConverter<TypedID2>]
//[EfCoreConverter<TypedID3>]
//etc.

[EfCoreConverter<AttendanceId>]

internal sealed partial class VogenEfCoreConverters;