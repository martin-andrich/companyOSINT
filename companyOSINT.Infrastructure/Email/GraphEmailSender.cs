using Azure.Identity;
using companyOSINT.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace companyOSINT.Infrastructure.Email;

public class GraphEmailSender(IOptions<GraphEmailSettings> options, ILogger<GraphEmailSender> logger)
    : IEmailSender<ApplicationUser>
{
    private readonly GraphEmailSettings _settings = options.Value;

    public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        var subject = "E-Mail-Adresse bestätigen - company-OSINT.com";
        var body = $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 32px;">
                <h2 style="color: #0c1222;">E-Mail-Adresse bestätigen</h2>
                <p style="color: #334155;">Hallo {user.FirstName},</p>
                <p style="color: #334155;">vielen Dank für Ihre Registrierung bei company-OSINT.com. Bitte bestätigen Sie Ihre E-Mail-Adresse, indem Sie auf den folgenden Button klicken:</p>
                <p style="text-align: center; margin: 32px 0;">
                    <a href="{confirmationLink}" style="background-color: #4f46e5; color: #ffffff; padding: 12px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; display: inline-block;">E-Mail bestätigen</a>
                </p>
                <p style="color: #64748b; font-size: 14px;">Falls der Button nicht funktioniert, kopieren Sie diesen Link in Ihren Browser:</p>
                <p style="color: #64748b; font-size: 14px; word-break: break-all;">{confirmationLink}</p>
                <hr style="border: none; border-top: 1px solid #e2e8f0; margin: 32px 0;" />
                <p style="color: #94a3b8; font-size: 12px;">Sie erhalten diese E-Mail, weil Sie sich bei company-OSINT.com registriert haben. Falls Sie sich nicht registriert haben, können Sie diese E-Mail ignorieren.</p>
            </div>
            """;

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        var subject = "Passwort zurücksetzen - company-OSINT.com";
        var body = $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 32px;">
                <h2 style="color: #0c1222;">Passwort zurücksetzen</h2>
                <p style="color: #334155;">Hallo {user.FirstName},</p>
                <p style="color: #334155;">Sie haben angefordert, Ihr Passwort zurückzusetzen. Klicken Sie auf den folgenden Button:</p>
                <p style="text-align: center; margin: 32px 0;">
                    <a href="{resetLink}" style="background-color: #4f46e5; color: #ffffff; padding: 12px 32px; text-decoration: none; border-radius: 8px; font-weight: 600; display: inline-block;">Passwort zurücksetzen</a>
                </p>
                <p style="color: #64748b; font-size: 14px;">Falls Sie kein neues Passwort angefordert haben, können Sie diese E-Mail ignorieren.</p>
            </div>
            """;

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var subject = "Passwort-Code - company-OSINT.com";
        var body = $"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; max-width: 600px; margin: 0 auto; padding: 32px;">
                <h2 style="color: #0c1222;">Ihr Passwort-Code</h2>
                <p style="color: #334155;">Hallo {user.FirstName},</p>
                <p style="color: #334155;">Ihr Code zum Zurücksetzen des Passworts lautet:</p>
                <p style="text-align: center; font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #4f46e5; margin: 32px 0;">{resetCode}</p>
                <p style="color: #64748b; font-size: 14px;">Falls Sie keinen Code angefordert haben, können Sie diese E-Mail ignorieren.</p>
            </div>
            """;

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var credential = new ClientSecretCredential(
                _settings.TenantId, _settings.ClientId, _settings.ClientSecret);

            var graphClient = new GraphServiceClient(credential);

            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = htmlBody
                },
                ToRecipients =
                [
                    new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = toEmail }
                    }
                ],
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = _settings.SenderEmail,
                        Name = _settings.SenderName
                    }
                }
            };

            await graphClient.Users[_settings.SenderEmail].SendMail
                .PostAsync(new SendMailPostRequestBody
                {
                    Message = message,
                    SaveToSentItems = false
                });

            logger.LogInformation("E-Mail erfolgreich gesendet an {Email} (Betreff: {Subject})", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Senden der E-Mail an {Email} (Betreff: {Subject})", toEmail, subject);
            throw;
        }
    }
}
