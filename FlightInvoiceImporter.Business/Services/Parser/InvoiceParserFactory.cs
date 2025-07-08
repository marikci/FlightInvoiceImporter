using FlightInvoiceImporter.Business.Interfaces.Parser;

namespace FlightInvoiceImporter.Business.Services.Parser;

public class InvoiceParserFactory : IInvoiceParserFactory
{
    private readonly PdfInvoiceParser _pdfParser;

    public InvoiceParserFactory(PdfInvoiceParser pdfParser)
    {
        _pdfParser = pdfParser;
    }

    /// <summary>
    /// Retrieves an appropriate invoice parser based on the specified file extension.
    /// </summary>
    public IInvoiceParser GetParser(string fileExtension)
    {
        return fileExtension.ToLower() switch
        {
            ".pdf" => _pdfParser,
            _ => throw new NotSupportedException($"Unsupported file extension: {fileExtension}")
        };
    }
}