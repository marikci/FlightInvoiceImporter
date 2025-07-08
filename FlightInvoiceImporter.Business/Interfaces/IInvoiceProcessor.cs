using FlightInvoiceImporter.Business.Interfaces.Parser;
using FlightInvoiceImporter.Models.Result;

namespace FlightInvoiceImporter.Business.Interfaces;

public interface IInvoiceProcessor
{
    Task<InvoiceProcessingResult> ProcessAsync(string filePath, IInvoiceParser parser);
}