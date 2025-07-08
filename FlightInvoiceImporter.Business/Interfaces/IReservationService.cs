
using FlightInvoiceImporter.Models.Reservation;

namespace FlightInvoiceImporter.Business.Interfaces;

public interface IReservationService
{
    Task<IEnumerable<ReservationModel>> GetReservationsByFlightAsync(string carrierCode, int flightNumber, DateTime flightDate);
    Task ExecuteInTransactionAsync(Func<Task> work);
    Task UpdateInvoiceNumbersAsync(IEnumerable<ReservationModel> updates);
}