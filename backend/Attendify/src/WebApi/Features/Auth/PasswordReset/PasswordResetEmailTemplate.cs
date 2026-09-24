using System.Reflection;
using System.Text.Encodings.Web;
using Attendify.Common.Email;

namespace Attendify.Features.Auth.PasswordReset;

internal static class PasswordResetEmailTemplate
{
    private const string ResourceName =
        "Attendify.Features.Auth.PasswordReset.Templates.PasswordResetCode.html";
    private const string LogoResourceName =
        "Attendify.Features.Auth.PasswordReset.Templates.Attendify-large.png";
    private const string LogoContentId = "attendify-logo";

    public static OutboundEmail Create(string recipient, string securityCode, int expiresInMinutes)
    {
        using Stream stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded email template '{ResourceName}' was not found."
            );
        using var reader = new StreamReader(stream);
        string template = reader.ReadToEnd();

        string safeCode = HtmlEncoder.Default.Encode(securityCode);
        string htmlBody = template
            .Replace("{{SecurityCode}}", safeCode, StringComparison.Ordinal)
            .Replace(
                "{{ExpiresInMinutes}}",
                expiresInMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                StringComparison.Ordinal
            );

        string textBody =
            $"Your Attendify password reset code is {securityCode}. It expires in {expiresInMinutes} minutes. If you did not request this, you can ignore this email.";

        return new OutboundEmail(recipient, "Reset your Attendify password", htmlBody, textBody)
        {
            Attachments =
            [
                new OutboundEmailAttachment(
                    "Attendify-large.png",
                    "image/png",
                    ReadLogo(),
                    LogoContentId
                ),
            ],
        };
    }

    private static byte[] ReadLogo()
    {
        using Stream stream =
            Assembly.GetExecutingAssembly().GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded email logo '{LogoResourceName}' was not found."
            );
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }
}
