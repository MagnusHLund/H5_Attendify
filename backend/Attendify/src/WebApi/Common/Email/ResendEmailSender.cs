using Microsoft.Extensions.Options;
using Resend;

namespace Attendify.Common.Email;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly EmailOptions _emailOptions;

    public ResendEmailSender(IResend resend, IOptions<EmailOptions> emailOptions)
    {
        _resend = resend;
        _emailOptions = emailOptions.Value;
    }

    public async Task SendAsync(OutboundEmail email, CancellationToken cancellationToken)
    {
        var message = new EmailMessage
        {
            From = $"{_emailOptions.FromName} <{_emailOptions.FromAddress}>",
            Subject = email.Subject,
            HtmlBody = email.HtmlBody,
            TextBody = email.TextBody,
        };
        message.To.Add(email.Recipient);

        if (email.Attachments.Count > 0)
            message.Attachments = [];

        foreach (OutboundEmailAttachment attachment in email.Attachments)
        {
            message?.Attachments?.Add(
                new EmailAttachment
                {
                    Filename = attachment.Filename,
                    ContentType = attachment.ContentType,
                    Content = attachment.Content,
                    ContentId = attachment.ContentId,
                }
            );
        }

        await _resend.EmailSendAsync(message, cancellationToken);
    }
}
