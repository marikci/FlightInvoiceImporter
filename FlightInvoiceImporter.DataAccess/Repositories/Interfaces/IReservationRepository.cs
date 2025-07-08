using FlightInvoiceImporter.DataAccess.Entities;

namespace FlightInvoiceImporter.DataAccess.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<List<ReservationEntity>> GetReservationsByFlightAsync(string carrierCode, int flightNo, DateTime flightDate);
    Task<ReservationEntity?> GetReservationAsync(long id);
    Task SaveChangesAsync();
}