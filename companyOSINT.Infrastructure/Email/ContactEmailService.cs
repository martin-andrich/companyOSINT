using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

namespace companyOSINT.Infrastructure.Email;

public interface IContactEmailService
{
    Task SendContactEmailAsync(string name, string email, string message);
}

public class ContactEmailService(
    IOptions<GraphEmailSettings> graphOptions,
    IConfiguration configuration,
    ILogger<ContactEmailService> logger) : IContactEmailService
{
    private readonly GraphEmailSettings _graph = graphOptions.Value;

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

        try
        {
            var credential = new ClientSecretCredential(
                _graph.TenantId, _graph.ClientId, _graph.ClientSecret);

            var graphClient = new GraphServiceClient(credential);

            var graphMessage = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients =
                [
                    new Recipient
                    {
                        EmailAddress = new EmailAddress { Address = recipientEmail }
                    }
                ],
                From = new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = _graph.SenderEmail,
                        Name = _graph.SenderName
                    }
                },
                ReplyTo =
                [
                    new Recipient
                    {
                        EmailAddress = new EmailAddress
                        {
                            Address = email,
                            Name = name
                        }
                    }
                ]
            };

            await graphClient.Users[_graph.SenderEmail].SendMail
                .PostAsync(new SendMailPostRequestBody
                {
                    Message = graphMessage,
                    SaveToSentItems = false
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fehler beim Senden der Kontakt-E-Mail von {Email}", email);
            throw;
        }
    }
}
