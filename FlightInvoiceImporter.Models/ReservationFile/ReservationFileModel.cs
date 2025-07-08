namespace FlightInvoiceImporter.Models.ReservationFile;

public class ReservationFileModel
{
    public long Id { get; set; }

    public string FileName { get; set; }

    public string UniqueFileName { get; set; }

    public long InvoiceNumber { get; set; }

    public string FilePath { get; set; }

    public int RowCount { get; set; }

    public long FileSizeInBytes { get; set; }

    public DateTime ProcessedAt { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public static ReservationFileModel Success(long invoiceNumber,string fileName, string extension, string filePath, int rowCount, long size)
    {
        return new ReservationFileModel
        {
            InvoiceNumber = invoiceNumber,
            FileName = fileName,
            FilePath = filePath,
            UniqueFileName = $"{Guid.NewGuid().ToString()}{extension}",
            RowCount = rowCount,
            ProcessedAt = DateTime.UtcNow,
            FileSizeInBytes = size,
            IsSuccess = true
        };
    }

    public static ReservationFileModel Fail(string fileName, string extension, string filePath, long size, string? errorMessage)
    {
        return new ReservationFileModel
        {
            FileName = fileName,
            FilePath = filePath,
            UniqueFileName = $"{Guid.NewGuid().ToString()}{extension}",
            FileSizeInBytes = size,
            ProcessedAt = DateTime.UtcNow,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}