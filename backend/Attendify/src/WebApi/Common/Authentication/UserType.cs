using System.Text.Json.Serialization;

namespace Attendify.Common.Authentication;

public enum UserType
{
    [JsonStringEnumMemberName("student")]
    Student,
    [JsonStringEnumMemberName("school_administrator")]
    SchoolAdministrator,
}
