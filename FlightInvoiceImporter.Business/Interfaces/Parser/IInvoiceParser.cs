using FlightInvoiceImporter.Models.FileInvoice;

namespace FlightInvoiceImporter.Business.Interfaces.Parser;

public interface IInvoiceParser
{
    FileInvoiceModel Parse(string filePath);
}