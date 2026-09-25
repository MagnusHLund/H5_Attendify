using System.ComponentModel.DataAnnotations;

namespace Attendify.Common.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required, EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    [Required]
    public string FromName { get; set; } = string.Empty;

}
