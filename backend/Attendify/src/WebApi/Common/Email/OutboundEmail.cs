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

public sealed record OutboundEmailAttachment(
    string Filename,
    string ContentType,
    byte[] Content,
    string? ContentId = null
);
