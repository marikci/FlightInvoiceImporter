namespace FlightInvoiceImporter.Business.Constants;

public static class FileConstants
{
    public const string INCOMING_FOLDER_NAME = "incoming";
    public const string PROCESSED_FOLDER_NAME = "processed";
    public const string ERROR_FOLDER_NAME = "error";
    public const string INVOICE_NO_PATTERN = @"Nummer\s*(\d+)";
    public const string CURRENCY_PATTERN = @"Summen in\s*([A-Z]{3})";

    public const string RESERVATION_RECORD_PATTERN = @"
             ^(?<Season>\d+)\s+
             (?<VT>\d+)\s+
             (?<FlightDate>\d{2}\.\d{2}\.\d{4})\s+
             (?<CarrierCode>[A-Z]{2})\s?(?<FlightNo>\d{3,4})\s+
             (?<Routing>(?:[A-Z]{3}(?:\s+)?)+)\s+
             (?<SoldSeats>\d+-?)\s+
             (?<UnitPrice>[\d\.]+,\d{2})\s+
             (?<TotalAmount>[\d\.]+,\d{2}-?)\s*
             (?<AmountSummary>[\d\.]+,\d{2}-?)?[\s]*$
            ";
}