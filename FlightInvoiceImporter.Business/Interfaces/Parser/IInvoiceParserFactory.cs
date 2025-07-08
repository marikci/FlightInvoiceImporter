namespace FlightInvoiceImporter.Business.Interfaces.Parser;

public interface IInvoiceParserFactory
{
    IInvoiceParser GetParser(string fileExtension);
}