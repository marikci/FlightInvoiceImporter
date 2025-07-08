using FlightInvoiceImporter.DataAccess.Entities;
using FlightInvoiceImporter.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlightInvoiceImporter.DataAccess.Repositories;

public class ReservationFileRepository : IReservationFileRepository
{
    private readonly ReservationDbContext _dbContext;

    public ReservationFileRepository(ReservationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ReservationFileEntity item)
    {
        await _dbContext.AddAsync(item);
    }

    public Task<bool> ExistsAsync(long invoiceNo)
    {
        return _dbContext.ReservationFiles.AnyAsync(x => x.InvoiceNumber == invoiceNo);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}