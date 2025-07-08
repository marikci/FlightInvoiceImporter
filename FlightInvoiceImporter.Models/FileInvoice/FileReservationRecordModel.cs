namespace FlightInvoiceImporter.Models.FileInvoice;

public class FileReservationRecordModel
{
    public long Id { get; set; }
    public string Season { get; set; } = string.Empty;
    public string VT { get; set; } = string.Empty;
    public DateTime FlightDate { get; set; }
    public string CarrierCode { get; set; } = string.Empty;
    public int FlightNo { get; set; }
    public string Routing { get; set; } = string.Empty;
    public int SoldSeats { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? AmountSummary { get; set; }
}