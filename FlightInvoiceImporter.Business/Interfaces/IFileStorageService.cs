namespace FlightInvoiceImporter.Business.Interfaces;

public interface IFileStorageService
{
    string TargetRootDirectory { get; }
    string SourceDirectory { get; }
    string IncomingDirectory { get; }
    string ProcessedDirectory { get; }
    string ErrorDirectory { get; }
    bool IsFileStable(string filePath, int delayMs = 1000);
    void CreateDirectory(string dir);
    bool IsExistsDirectory(string dir);
    string MoveToIncoming(string filePath);
    string MoveToProcessed(string filePath, string fileName);
    string MoveToError(string filePath, string fileName);
    MemoryStream CreateCsvStream<T>(IEnumerable<T> records);
}