using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace companyOSINT.Infrastructure.Email;

public interface IContactEmailService
{
    Task SendContactEmailAsync(string name, string email, string message);
}

public class ContactEmailService(
    IOptions<SmtpSettings> smtpOptions,
    IConfiguration configuration,
    ILogger<ContactEmailService> logger) : IContactEmailService
{
    private readonly SmtpSettings _smtp = smtpOptions.Value;

    public async Task SendContactEmailAsync(string name, string email, string message)
    {
        var recipientEmail = configuration["ContactEmail"] ?? "info@company-osint.com";

        var subject = $"Kontaktanfrage von {name}";
        var body = $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 32px;">
                <h2 style="color: #0c1222;">Neue Kontaktanfrage</h2>
                <table style="width: 100%; border-collapse: collapse; margin: 24px 0;">
                    <tr>
                        <td style="padding: 8px 0; color: #64748b; width: 100px;">Name:</td>
                        <td style="padding: 8px 0; color: #334155;">{System.Net.WebUtility.HtmlEncode(name)}</td>
                    </tr>
                    <tr>
                        <td style="padding: 8px 0; color: #64748b;">E-Mail:</td>
                        <td style="padding: 8px 0; color: #334155;"><a href="mailto:{System.Net.WebUtility.HtmlEncode(email)}" style="color: #4f46e5;">{System.Net.WebUtility.HtmlEncode(email)}</a></td>
                    </tr>
                </table>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 16px 0;" />
                <div style="color: #334155; white-space: pre-wrap;">{System.Net.WebUtility.HtmlEncode(message)}</div>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 24px 0;" />
                <p style="color: #94a3b8; font-size: 12px;">Diese Nachricht wurde über das Kontaktformular auf company-OSINT.com gesendet.</p>
            </div>
            """;

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_smtp.SenderName, _smtp.SenderEmail));
        mimeMessage.To.Add(MailboxAddress.Parse(recipientEmail));
        mimeMessage.ReplyTo.Add(new MailboxAddress(name, email));
        mimeMessage.Subject = subject;
        mimeMessage.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtp.UserName, _smtp.Password);
            await client.SendAsync(mimeMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Senden der Kontakt-E-Mail von {Email}", email);
            throw;
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
