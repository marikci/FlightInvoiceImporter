namespace FlightInvoiceImporter.Models.FileInvoice;

public class FileInvoiceModel
{
    public long InvoiceNo { get; set; }
    public string Currency { get; set; }
    public List<FileReservationRecordModel> Reservations { get; set; } = new ();
}