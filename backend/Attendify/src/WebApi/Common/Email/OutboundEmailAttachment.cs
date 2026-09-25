namespace Attendify.Common.Email;

public sealed record OutboundEmailAttachment(
    string Filename,
    string ContentType,
    byte[] Content,
    string? ContentId = null
);
