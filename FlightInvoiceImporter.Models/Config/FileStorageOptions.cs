namespace FlightInvoiceImporter.Models.Config;

public class FileStorageOptions
{
    public required string TargetRootDirectory { get; set; }
    public required string SourceDirectory { get; set; }
}