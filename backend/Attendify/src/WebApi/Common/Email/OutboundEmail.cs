namespace Attendify.Common.Email;

public sealed record OutboundEmail(
    string Recipient,
    string Subject,
    string HtmlBody,
    string TextBody
)
{
    public IReadOnlyList<OutboundEmailAttachment> Attachments { get; init; } = [];
}
