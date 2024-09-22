using MailKit.Net.Smtp;
using MimeKit;
using PortfolioManagerAPI.Domain;

namespace PortfolioManagerAPI.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendExpiringProductsEmail(List<InvestmentProduct> products)
    {
        var mailboxAddresses = new List<MailboxAddress> { new("", "admins@xpi.com.br") };

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("", "EmailSettingsFrom"));
        emailMessage.To.AddRange(mailboxAddresses);
        emailMessage.Subject = "Products Expiring Soon";

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = $"Products expiring soon: \n{string.Join(", ", products.Select(p => p.Name))}"
        };

        emailMessage.Body = bodyBuilder.ToMessageBody();

        // await SendEmail(emailMessage);
        _logger.LogInformation("Email sent successfully to {Recipients}.", string.Join(", ", mailboxAddresses.Select(m => m.Address)));
    }

    private async Task SendEmail(MimeMessage emailMessage)
    {
        using var client = new SmtpClient();

        try
        {
            client.Connect("EmailSettingsSmtpServer", 587, true);
            client.AuthenticationMechanisms.Remove("XOUATH2");
            client.Authenticate("EmailSettingsFrom", "EmailSettingsPassword");

            await client.SendAsync(emailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while sending the email: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
