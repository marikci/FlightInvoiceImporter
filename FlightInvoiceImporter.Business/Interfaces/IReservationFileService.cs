using FlightInvoiceImporter.Models.ReservationFile;
namespace FlightInvoiceImporter.Business.Interfaces;

public interface IReservationFileService
{
    Task CreateReservationFile(ReservationFileModel reservationFile);
    Task<bool> IsFileAppliedAsync(long invoiceNo);
}