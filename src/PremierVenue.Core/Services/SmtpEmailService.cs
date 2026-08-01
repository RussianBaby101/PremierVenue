using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PremierVenue.Core.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        var host = _configuration["Smtp:Host"];
        var portValue = _configuration["Smtp:Port"];
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "PremierVenue";
        var timeoutValue = _configuration["Smtp:TimeoutSeconds"];

        var timeoutSeconds = int.TryParse(timeoutValue, out var configuredTimeoutSeconds) && configuredTimeoutSeconds > 0
            ? configuredTimeoutSeconds
            : 30;

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromEmail) ||
            !int.TryParse(portValue, out var port))
        {
            const string configurationError = "SMTP is not fully configured.";
            _logger.LogError("{Message} Email to {To} with subject {Subject} was not sent.", configurationError, to, subject);
            throw new InvalidOperationException(configurationError);
        }

        using var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
            Timeout = timeoutSeconds * 1000
        };

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = isHtml
        };
        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message).WaitAsync(TimeSpan.FromSeconds(timeoutSeconds));
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timed out after {TimeoutSeconds}s sending email to {To} with subject {Subject}", timeoutSeconds, to, subject);
            throw new InvalidOperationException("Email service timed out while sending the message. Please try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            throw;
        }
    }
}
