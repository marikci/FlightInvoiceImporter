using FlightInvoiceImporter.Business.Helpers;
using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.Business.Interfaces.Parser;
using FlightInvoiceImporter.Models;
using FlightInvoiceImporter.Models.Config;
using FlightInvoiceImporter.Models.FileInvoice;
using FlightInvoiceImporter.Models.Process;
using FlightInvoiceImporter.Models.Reservation;
using FlightInvoiceImporter.Models.Result;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RazorLight;

namespace FlightInvoiceImporter.Business.Services;

public class InvoiceProcessor : IInvoiceProcessor
{
    private readonly IFileStorageService _fileStorageService;
    private readonly InvoiceReportOptions _invoiceReportOptions;
    private readonly ILogger<InvoiceProcessor> _logger;
    private readonly IMailService _mailService;
    private readonly IRazorLightEngine _razor;
    private readonly IReservationService _reservationService;
    private readonly IReservationFileService _reservationFileService;
    private readonly List<Func<ProcessingRowContext, Task>> _rowHandlers;

    public InvoiceProcessor(IOptions<InvoiceReportOptions> invoiceReportOptions, ILogger<InvoiceProcessor> logger,
        IRazorLightEngine razor, IReservationService reservationService, IMailService mailService,
        IFileStorageService fileStorageService, IReservationFileService reservationFileService)
    {
        _logger = logger;
        _reservationService = reservationService;
        _mailService = mailService;
        _fileStorageService = fileStorageService;
        _invoiceReportOptions = invoiceReportOptions.Value;
        _razor = razor;

        _rowHandlers = [
            LoadFlightReservationsAsync,
            IdentifyDuplicateReservationsAsync,
            IdentifyAvailableReservationsAsync,
            AssignReservationsAsync,
            ValidateSeatCountsAsync,
            ValidatePriceAsync
        ];
        _reservationFileService = reservationFileService;
    }

    public async Task<InvoiceProcessingResult> ProcessAsync(string filePath, IInvoiceParser parser)
    {
        var parsedInvoice = parser.Parse(filePath);
        var fileAppliedBefore = await _reservationFileService.IsFileAppliedAsync(parsedInvoice.InvoiceNo);

        if (fileAppliedBefore)
        {
            _logger.LogWarning("File already applied: {FilePath}", filePath);
            return InvoiceProcessingResult.Fail("File already applied.");
        }


        var soldSeatReservations = parsedInvoice.Reservations.Where(r => r.SoldSeats > 0).ToList();
        if (!soldSeatReservations.Any())
        {
            _logger.LogWarning("No invoice rows in file: {FilePath}", filePath);
            return InvoiceProcessingResult.Fail("No invoice rows.");
        }

        var report = new ProcessingReport
        {
            InvoiceNo = parsedInvoice.InvoiceNo,
            RowsCount = soldSeatReservations.Count,
            Currency = parsedInvoice.Currency
        };
        await _reservationService.ExecuteInTransactionAsync(async () =>
        {
            foreach (var row in soldSeatReservations)
            {
                var ctx = new ProcessingRowContext(row, parsedInvoice.InvoiceNo, report);
                foreach (var handler in _rowHandlers)
                {
                    await handler(ctx);
                    if (ctx.Handled)
                    {
                        break;
                    }
                }
            }

            await _reservationService.UpdateInvoiceNumbersAsync(report.MatchedReservations.Select(m => ReservationModel.InvoiceUpdate(m.Id, m.InvoiceNumber)));
        });

        await GenerateAndSendReportAsync(report, parsedInvoice.InvoiceNo);

        return InvoiceProcessingResult.Success(parsedInvoice.InvoiceNo, soldSeatReservations.Count);
    }
    /// <summary>
    /// Loads flight reservations for the specified flight and updates the processing context with the results.
    /// </summary>
    private async Task LoadFlightReservationsAsync(ProcessingRowContext ctx)
    {
        var row = ctx.Row;
        var reservations = await _reservationService.GetReservationsByFlightAsync(row.CarrierCode, row.FlightNo, row.FlightDate);
        ctx.FlightReservations = reservations.ToList();
        if (!reservations.Any())
        {
            ctx.Report.UnmatchedReservationRows.Add(row);
            ctx.Handled = true;
        }
    }

    /// <summary>
    /// Identifies duplicate flight reservations within the provided processing context.
    /// </summary>
    private Task IdentifyDuplicateReservationsAsync(ProcessingRowContext ctx)
    {
        var duplicateReservations = ctx.FlightReservations.Where(c => c.InvoiceNumber != null && c.InvoiceNumber != ctx.InvoiceNo).ToList();
        ctx.DuplicateFlightReservations = duplicateReservations;
        ctx.Report.DuplicateReservations.AddRange(duplicateReservations);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Identifies and processes flight reservations that are not yet associated with an invoice.
    /// </summary>
    private Task IdentifyAvailableReservationsAsync(ProcessingRowContext ctx)
    {
        ctx.PendingReservations = ctx.FlightReservations.Where(c => c.InvoiceNumber == null).OrderBy(c => c.Id).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Assigns reservations to the current processing context based on the number of sold seats.
    /// </summary>
    private Task AssignReservationsAsync(ProcessingRowContext ctx)
    {
        var needSeatCount = ctx.Row.SoldSeats;
        var pickedReservations = new List<ReservationModel>();
        for (var i = 0; i < needSeatCount && i < ctx.PendingReservations.Count; i++)
        {
            var reservation = ctx.PendingReservations[i];
            reservation.InvoiceNumber = ctx.InvoiceNo;
            pickedReservations.Add(reservation);
        }

        ctx.AssignedReservations = pickedReservations;
        ctx.Report.MatchedReservations.AddRange(pickedReservations);

        return Task.CompletedTask;

    }
     
    /// <summary>
    /// Validates that the number of assigned reservations matches or exceeds the number of sold seats for the specified
    /// row context.
    /// </summary>
    private Task ValidateSeatCountsAsync(ProcessingRowContext ctx)
    {
        if (ctx.AssignedReservations.Count < ctx.Row.SoldSeats)
        {
            ctx.Report.UnmatchedReservationRows.Add(ctx.Row);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Validates the total price of assigned reservations against the expected total amount.
    /// </summary>
    private Task ValidatePriceAsync(ProcessingRowContext ctx)
    {
        foreach (var res in ctx.AssignedReservations)
        {
            if (res.Price != ctx.Row.UnitPrice)
            {
                ctx.Report.PriceMismatchReservationRows.Add(ctx.Row);
            }
        }

        return Task.CompletedTask;
    }


    private async Task GenerateAndSendReportAsync(ProcessingReport report, long invoiceNo)
    {
        var reportViewModel = new InvoiceReportViewModel
        {
            Currency = report.Currency,
            CultureInfo = CurrencyCultureProvider.GetCulture(report.Currency),
            InvoiceNo = invoiceNo,
            TotalRows = report.RowsCount,
            FullyMatched = report.MatchedReservations.Count,

            UnmatchedCount = report.UnmatchedReservationRows.Count,
            UnmatchedTotal = report.UnmatchedReservationRows.Sum(r => r.TotalAmount),

            DuplicateCount = report.DuplicateReservations.Count,
            DuplicateTotal = report.DuplicateReservations.Sum(r => r.Price),

            PriceMismatchCount = report.PriceMismatchReservationRows.Count,
            PriceMismatchTotal = report.PriceMismatchReservationRows.Sum(r => r.TotalAmount)
        };

        var body = await _razor.CompileRenderAsync("InvoiceReportTemplate.cshtml", reportViewModel);

        var attachments = new List<MailAttachmentModel>();

        if (report.UnmatchedReservationRows.Any())
        {
            attachments.Add(new MailAttachmentModel
            {
                FileName = "unmatched.csv",
                Content = _fileStorageService.CreateCsvStream(report.UnmatchedReservationRows),
                ContentType = "text/csv"
            });
        }

        if (report.DuplicateReservations.Any())
        {
            attachments.Add(new MailAttachmentModel
            {
                FileName = "duplicates.csv",
                Content = _fileStorageService.CreateCsvStream(report.DuplicateReservations),
                ContentType = "text/csv"
            });
        }

        if (report.PriceMismatchReservationRows.Any())
        {
            attachments.Add(new MailAttachmentModel
            {
                FileName = "price_mismatches.csv",
                Content = _fileStorageService.CreateCsvStream(report.PriceMismatchReservationRows),
                ContentType = "text/csv"
            });
        }

        await _mailService.SendEmailAsync(
            _invoiceReportOptions.MailRecipients,
            $"Invoice Report – #{invoiceNo}",
            body,
            attachments
        );
    }
}