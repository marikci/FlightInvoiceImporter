using System.Globalization;
using System.Text;
using CsvHelper;
using FlightInvoiceImporter.Business.Constants;
using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.Models.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlightInvoiceImporter.Business.Services;

public class FileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _fileStorageOptions;

    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger, IOptions<FileStorageOptions> fileStorageOptions)
    {
        _logger = logger;
        _fileStorageOptions = fileStorageOptions.Value;
    }

    public string SourceDirectory => _fileStorageOptions.SourceDirectory;

    public string TargetRootDirectory => _fileStorageOptions.TargetRootDirectory;

    public string IncomingDirectory =>
        Path.Combine(_fileStorageOptions.TargetRootDirectory, FileConstants.INCOMING_FOLDER_NAME);

    public string ProcessedDirectory =>
        Path.Combine(_fileStorageOptions.TargetRootDirectory, FileConstants.PROCESSED_FOLDER_NAME);

    public string ErrorDirectory =>
        Path.Combine(_fileStorageOptions.TargetRootDirectory, FileConstants.ERROR_FOLDER_NAME);

    /// <summary>
    /// Determines whether the specified file is stable, meaning its size has not changed over a specified delay period.
    /// </summary>
    public bool IsFileStable(string filePath, int delayMs = 1000)
    {
        if (!File.Exists(filePath))
        {
            return false;
        }

        var fileInfo = new FileInfo(filePath);
        var sizeBefore = fileInfo.Length;

        Thread.Sleep(delayMs);

        fileInfo.Refresh();
        var sizeAfter = fileInfo.Length;

        var isStable = sizeBefore == sizeAfter;

        if (!isStable)
        {
            _logger.LogWarning("IsFileStable for {file}: {result} (before: {before}, after: {after})", filePath, isStable, sizeBefore, sizeAfter);
        }
        return isStable;
    }

    public string MoveToIncoming(string filePath)
    {
        return MoveFile(filePath, IncomingDirectory);
    }

    public string MoveToProcessed(string filePath, string fileName)
    {
        return MoveFile(filePath, ProcessedDirectory, fileName);
    }

    public string MoveToError(string filePath, string fileName)
    {
        return MoveFile(filePath, ErrorDirectory, fileName);
    }

    public void CreateDirectory(string dir)
    {
        Directory.CreateDirectory(dir);
    }

    public bool IsExistsDirectory(string dir)
    {
        return Directory.Exists(dir);
    }

    public MemoryStream CreateCsvStream<T>(IEnumerable<T> records)
    {
        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(records);
        }

        ms.Position = 0;
        return ms;
    }

    private string MoveFile(string filePath, string destinationDir, string? newFileName = null)
    {
        if (!Directory.Exists(destinationDir))
        {
            CreateDirectory(destinationDir);
        }

        var fileName = newFileName ?? Path.GetFileName(filePath);
        var targetPath = Path.Combine(destinationDir, fileName);

        File.Move(filePath, targetPath);
        return targetPath;
    }
}