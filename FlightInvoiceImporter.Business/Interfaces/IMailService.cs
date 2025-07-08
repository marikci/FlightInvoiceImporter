using FlightInvoiceImporter.Models;

namespace FlightInvoiceImporter.Business.Interfaces;

public interface IMailService
{
    Task SendEmailAsync(List<string> recipients, string subject, string htmlBody, IEnumerable<MailAttachmentModel?>? attachments = null);
}