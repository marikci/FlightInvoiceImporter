using System.Globalization;
using System.Text.RegularExpressions;
using FlightInvoiceImporter.Business.Constants;
using FlightInvoiceImporter.Business.Interfaces.Parser;
using FlightInvoiceImporter.Models.FileInvoice;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;

namespace FlightInvoiceImporter.Business.Services.Parser;

public class PdfInvoiceParser : IInvoiceParser
{
    private readonly ILogger<PdfInvoiceParser> _logger;

    public PdfInvoiceParser(ILogger<PdfInvoiceParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses a PDF file containing invoice data
    /// </summary>
    public FileInvoiceModel Parse(string filePath)
    {
        var invoice = new FileInvoiceModel();
        var reservationRecordRegex = new Regex(FileConstants.RESERVATION_RECORD_PATTERN, RegexOptions.IgnorePatternWhitespace);

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            var words = page.GetWords().ToList();
            if (!words.Any())
            {
                _logger.LogWarning("No words found on page {pageNumber}", page.Number);
                continue;
            }

            var pageText = page.Text;
            var invoiceNoMatch = Regex.Match(pageText, FileConstants.INVOICE_NO_PATTERN);
            if (invoiceNoMatch.Success)
            {
                var invoiceNo = invoiceNoMatch.Groups[1].Value;
                invoice.InvoiceNo = int.Parse(invoiceNo);
            }

            var currencyMatch = Regex.Match(pageText, FileConstants.CURRENCY_PATTERN);
            if (currencyMatch.Success)
            {
                var currency = currencyMatch.Groups[1].Value;
                invoice.Currency = currency;
            }

            var lines = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))
                .OrderByDescending(g => g.Key)
                .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text.Trim())))
                .ToList();

            var isHeaderSkipped = false;

            foreach (var line in lines)
            {
                var lineText = line.Trim();
                if (string.IsNullOrWhiteSpace(lineText))
                {
                    continue;
                }

                if (!isHeaderSkipped)
                {
                    isHeaderSkipped = true;
                    continue;
                }

                var reservationRecordMatch = reservationRecordRegex.Match(lineText);
                if (!reservationRecordMatch.Success)
                {
                    continue;
                }

                try
                {
                    var season = reservationRecordMatch.Groups["Season"].Value;
                    var vt = reservationRecordMatch.Groups["VT"].Value;

                    var flightDate = DateTime.ParseExact(reservationRecordMatch.Groups["FlightDate"].Value, "dd.MM.yyyy", CultureInfo.InvariantCulture);

                    var carrierCode = reservationRecordMatch.Groups["CarrierCode"].Value;

                    var flightNoRaw = reservationRecordMatch.Groups["FlightNo"].Value;

                    var flightNo = int.Parse(flightNoRaw);


                    var routing = reservationRecordMatch.Groups["Routing"].Value.Trim();

                    var soldSeatsRaw = reservationRecordMatch.Groups["SoldSeats"].Value;
                    var soldSeats = int.Parse(soldSeatsRaw.Replace("-", ""));
                    if (soldSeatsRaw.EndsWith("-"))
                    {
                        soldSeats *= -1;
                    }

                    var unitPrice = decimal.Parse(reservationRecordMatch.Groups["UnitPrice"].Value.Replace(",", "."),
                        CultureInfo.InvariantCulture);

                    var totalAmountRaw = reservationRecordMatch.Groups["TotalAmount"].Value;
                    var totalAmount = decimal.Parse(totalAmountRaw.Replace(",", ".").Replace("-", ""),
                        CultureInfo.InvariantCulture);
                    if (totalAmountRaw.EndsWith("-"))
                    {
                        totalAmount *= -1;
                    }

                    var amountSummaryRaw = reservationRecordMatch.Groups["AmountSummary"].Value;
                    decimal? amountSummary = null;
                    if (!string.IsNullOrWhiteSpace(amountSummaryRaw))
                    {
                        amountSummary = decimal.Parse(amountSummaryRaw.Replace(".", "").Replace(",", ".").Replace("-", ""), CultureInfo.InvariantCulture);
                        if (amountSummaryRaw.EndsWith("-"))
                        {
                            amountSummary *= -1;
                        }
                    }

                    var reservationRecord = new FileReservationRecordModel
                    {
                        Season = season,
                        VT = vt,
                        FlightDate = DateTime.SpecifyKind(flightDate, DateTimeKind.Utc),
                        CarrierCode = carrierCode,
                        FlightNo = flightNo,
                        Routing = routing,
                        SoldSeats = soldSeats,
                        UnitPrice = unitPrice,
                        TotalAmount = totalAmount,
                        AmountSummary = amountSummary
                    };
                    invoice.Reservations.Add(reservationRecord);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing line: {lineText}", lineText);
                }
            }
        }

        _logger.LogInformation("Total parsed rows: {count} from file: {file}", invoice.Reservations.Count, filePath);

        return invoice;
    }
}