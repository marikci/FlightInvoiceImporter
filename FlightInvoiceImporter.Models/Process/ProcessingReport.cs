using FlightInvoiceImporter.Models.FileInvoice;
using FlightInvoiceImporter.Models.Reservation;

namespace FlightInvoiceImporter.Models.Process;

public class ProcessingReport
{
    public long InvoiceNo { get; set; }
    public int RowsCount { get; set; }
    public string? Currency { get; set; }
    public List<ReservationModel> MatchedReservations { get; } = new();
    public List<ReservationModel> DuplicateReservations { get; } = new();
    public List<FileReservationRecordModel> UnmatchedReservationRows { get; } = new();
    public List<FileReservationRecordModel> PriceMismatchReservationRows { get; } = new();
}