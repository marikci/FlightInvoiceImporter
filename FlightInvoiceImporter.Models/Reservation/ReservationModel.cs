namespace FlightInvoiceImporter.Models.Reservation;

public class ReservationModel
{
    public long Id { get; set; }

    public string BookingId { get; set; } = string.Empty;

    public string Customer { get; set; } = string.Empty;

    public string CarrierCode { get; set; } = string.Empty;

    public int FlightNumber { get; set; }

    public DateTime FlightDate { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string Destination { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public long? InvoiceNumber { get; set; }

    public static ReservationModel InvoiceUpdate(long id, long? invoiceNumber)
    {
        return new ReservationModel { Id = id, InvoiceNumber = invoiceNumber };
    }
}