using AutoMapper;
using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.DataAccess.Entities;
using FlightInvoiceImporter.DataAccess.Repositories.Interfaces;
using FlightInvoiceImporter.Models.ReservationFile;
using Microsoft.Extensions.Logging;

namespace FlightInvoiceImporter.Business.Services;

public class ReservationFileService : IReservationFileService
{
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationFileService> _logger;
    private readonly IReservationFileRepository _repository;

    public ReservationFileService(ILogger<ReservationFileService> logger, IReservationFileRepository repository, IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
        _repository = repository;
    }

    public async Task CreateReservationFile(ReservationFileModel reservationFile)
    {
        var entity = _mapper.Map<ReservationFileEntity>(reservationFile);
        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
    }

    /// <summary>
    /// Determines whether a file associated with the specified invoice number has been applied.
    /// </summary>
    public async Task<bool> IsFileAppliedAsync(long invoiceNo)
    {
        return await _repository.ExistsAsync(invoiceNo);
    }
}