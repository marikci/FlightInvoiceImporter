using AutoMapper;
using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.DataAccess;
using FlightInvoiceImporter.DataAccess.Repositories.Interfaces;
using FlightInvoiceImporter.Models.Reservation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightInvoiceImporter.Business.Services;

public class ReservationService : IReservationService, IDisposable, IAsyncDisposable
{
    private readonly IMapper _mapper;
    private readonly ReservationDbContext _dbContext;
    private readonly ILogger<ReservationService> _logger;
    private readonly IReservationRepository _repository;

    public ReservationService(ReservationDbContext dbContext, ILogger<ReservationService> logger,
        IReservationRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _logger = logger;
        _repository = repository;
    }


    public async Task ExecuteInTransactionAsync(Func<Task> work)
    {
        //Retrieve mechanism
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await work();
                //await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                throw;
            }
            
        });
    }

    /// <summary>
    /// Retrieves a collection of reservations for a specific flight.
    /// </summary>
    public async Task<IEnumerable<ReservationModel>> GetReservationsByFlightAsync(string carrierCode, int flightNumber, DateTime flightDate)
    {
        var entities = await _repository.GetReservationsByFlightAsync(carrierCode, flightNumber, flightDate);
        return entities.Select(x => _mapper.Map<ReservationModel>(x));
    }

    /// <summary>
    /// Updates the invoice numbers for the specified reservations asynchronously.
    /// </summary>
    public async Task UpdateInvoiceNumbersAsync(IEnumerable<ReservationModel> updates)
    {
        foreach (var u in updates)
        {
            var reservationEntity = await _repository.GetReservationAsync(u.Id);
            if (reservationEntity != null)
            {
                reservationEntity.InvoiceNumber = u.InvoiceNumber;
            }
            else
            {
                _logger.LogWarning("Entity not found for invoice update: {Id}", u.Id);
            }
                
        }

        await _repository.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
    }

    public void Dispose()
    {
    }
}