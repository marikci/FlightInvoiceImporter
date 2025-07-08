namespace FlightInvoiceImporter.Models.Config;

public class MailOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
}