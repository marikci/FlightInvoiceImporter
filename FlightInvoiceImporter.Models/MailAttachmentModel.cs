namespace FlightInvoiceImporter.Models;

public class MailAttachmentModel
{
    public string FileName { get; set; }
    public Stream Content { get; set; }
    public string ContentType { get; set; } = "application/octet-stream";
}