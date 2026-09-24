namespace Attendify.Common.Email;

public interface IEmailSender
{
    Task SendAsync(OutboundEmail email, CancellationToken cancellationToken);
}
