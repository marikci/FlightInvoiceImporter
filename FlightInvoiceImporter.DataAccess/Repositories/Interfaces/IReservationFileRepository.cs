using FlightInvoiceImporter.DataAccess.Entities;

namespace FlightInvoiceImporter.DataAccess.Repositories.Interfaces;

public interface IReservationFileRepository
{
    Task AddAsync(ReservationFileEntity item);
    Task SaveChangesAsync();
    Task<bool> ExistsAsync(long invoiceNo);
}