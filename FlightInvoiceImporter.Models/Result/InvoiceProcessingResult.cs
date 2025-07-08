namespace FlightInvoiceImporter.Models.Result;

public class InvoiceProcessingResult
{
    public bool IsSuccess { get; set; }
    public long InvoiceNumber { get; set; }
    public int RowCount { get; set; }
    public string? ErrorMessage { get; set; }

    public static InvoiceProcessingResult Success(long invoiceNumber, int rowCount)
    {
        return new InvoiceProcessingResult { InvoiceNumber = invoiceNumber,IsSuccess = true, RowCount = rowCount };
    }

    public static InvoiceProcessingResult Fail(string errorMessage)
    {
        return new InvoiceProcessingResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}