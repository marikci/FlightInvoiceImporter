using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.Models;
using FlightInvoiceImporter.Models.Config;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FlightInvoiceImporter.Business.Services;

public class MailService : IMailService, IDisposable
{
    private readonly ILogger<MailService> _logger;
    private readonly MailOptions _options;
    private readonly SmtpClient _smtp;

    public MailService(IOptions<MailOptions> options, ILogger<MailService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _smtp = new SmtpClient();
    }

    public async Task SendEmailAsync(List<string> recipients, string subject, string htmlBody, IEnumerable<MailAttachmentModel?>? attachments = null)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.User, _options.FromEmail));
        foreach (var recipient in recipients) message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                if (attachment.Content.CanSeek)
                {
                    attachment.Content.Position = 0;
                }
                await builder.Attachments.AddAsync(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }
        }
          
        message.Body = builder.ToMessageBody();

        try
        {
            await _smtp.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);

            await _smtp.AuthenticateAsync(_options.User, _options.Password);

            await _smtp.SendAsync(message);
            await _smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {ToEmail}", string.Join(",", recipients));
            throw;
        }
    }

    public void Dispose()
    {
        _smtp?.Dispose();
    }
}