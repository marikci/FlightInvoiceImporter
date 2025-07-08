using System.Globalization;

namespace FlightInvoiceImporter.Models.Process;

public class InvoiceReportViewModel
{
    public long InvoiceNo { get; set; }
    public string? Currency { get; set; }
    public CultureInfo CultureInfo { get; set; }
    public int TotalRows { get; set; }
    public int FullyMatched { get; set; }

    public int UnmatchedCount { get; set; }
    public decimal UnmatchedTotal { get; set; }

    public int DuplicateCount { get; set; }
    public decimal DuplicateTotal { get; set; }

    public int PriceMismatchCount { get; set; }
    public decimal PriceMismatchTotal { get; set; }
}