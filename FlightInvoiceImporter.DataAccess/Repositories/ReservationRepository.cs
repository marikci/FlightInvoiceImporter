using FlightInvoiceImporter.DataAccess.Entities;
using FlightInvoiceImporter.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightInvoiceImporter.DataAccess.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly ReservationDbContext _dbContext;

    public ReservationRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReservationEntity?> GetReservationAsync(long id)
    {
        return await _dbContext.Reservations.FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<ReservationEntity>> GetReservationsByFlightAsync(string carrierCode, int flightNo, DateTime flightDate)
    {
        return await _dbContext.Reservations
            .Where(r => r.CarrierCode == carrierCode && r.FlightNumber == flightNo && r.FlightDate == flightDate)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public void Update(ReservationEntity item)
    {
        _dbContext.Update(item);
    }
}