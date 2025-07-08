using FlightInvoiceImporter.Models.FileInvoice;
using FlightInvoiceImporter.Models.Reservation;

namespace FlightInvoiceImporter.Models.Process;

public class ProcessingRowContext
{
    public ProcessingRowContext(FileReservationRecordModel row, long invoiceNo, ProcessingReport report)
    {
        Row = row;
        InvoiceNo = invoiceNo;
        Report = report;
    }

    public FileReservationRecordModel Row { get; }
    public long InvoiceNo { get; }
    public ProcessingReport Report { get; }
    public List<ReservationModel> FlightReservations { get; set; } = new();
    public List<ReservationModel> PendingReservations { get; set; } = new();
    public List<ReservationModel> AssignedReservations { get; set; } = new();
    public List<ReservationModel> DuplicateFlightReservations { get; set; } = new();
    public bool Handled { get; set; }
}