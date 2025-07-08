using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.Business.Interfaces.Parser;
using FlightInvoiceImporter.Models.ReservationFile;
using System.Diagnostics;
using System.Text;

namespace FlightInvoiceImporter.Worker;

public class InvoiceImportWorker : BackgroundService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<InvoiceImportWorker> _logger;
    private readonly IInvoiceParserFactory _parserFactory;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public InvoiceImportWorker(ILogger<InvoiceImportWorker> logger,
        IFileStorageService fileStorageService,
        IInvoiceParserFactory parserFactory,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _parserFactory = parserFactory;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PrintStartupBanner();
        Console.WriteLine($"Invoice Import Worker is working...");
        while (!stoppingToken.IsCancellationRequested)
        {
            ValidateFolders();
            ProcessIncomingFiles();
            ProcessSftpFiles();
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void PrintStartupBanner()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine(@"
        
   ____  ______ _____ _____ 
  / __ \|  ____|_   _|_   _|
 | |  | | |__    | |   | |  
 | |  | |  __|   | |   | |  
 | |__| | |     _| |_ _| |_ 
  \____/|_|    |_____|_____| v1.0
                        
 Odeon Flight Invoice Importer
            ");
    }

    /// <summary>
    /// Validates the existence of required directories for file storage operations.
    /// </summary>
    /// <exception cref="Exception">Thrown if the target root directory or the SFTP directory does not exist.</exception>
    private void ValidateFolders()
    {
        if (!Directory.Exists(_fileStorageService.TargetRootDirectory))
        {
            throw new Exception($"Target directory({_fileStorageService.TargetRootDirectory}) not found!");
        }

        if (!Directory.Exists(_fileStorageService.SourceDirectory))
        {
            throw new Exception($"Sftp directory({_fileStorageService.SourceDirectory}) not found!");
        }
    }

    /// <summary>
    /// Processes all files in the incoming directory by delegating each file to processing method.
    /// </summary>
    private void ProcessIncomingFiles()
    {
        if (!Directory.Exists(_fileStorageService.IncomingDirectory))
        {
            return;
        }

        var incomingFiles = Directory.GetFiles(_fileStorageService.IncomingDirectory);

        foreach (var filePath in incomingFiles)
        {
            _logger.LogInformation("Found unprocessed file in incoming: {file}", filePath);
            ProcessFileWithLogging(filePath);

        }
    }

    /// <summary>
    /// Processes files from the SFTP directory by verifying their stability, moving them to the incoming directory,
    /// and processing each file.
    /// </summary>
    private void ProcessSftpFiles()
    {
        var pdfFiles = Directory.GetFiles(_fileStorageService.SourceDirectory);

        foreach (var filePath in pdfFiles)
        {
            if (!_fileStorageService.IsFileStable(filePath))
            {
                _logger.LogInformation("File {file} is not stable yet. Skipping.", filePath);
                continue;
            }

            var movedFilePath = _fileStorageService.MoveToIncoming(filePath);
            ProcessFileWithLogging(movedFilePath);
        }
    }

    private void ProcessFileWithLogging(string filePath)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("▶ Begin processing: {File}", Path.GetFileName(filePath));

        try
        {
            ProcessSingleFileAsync(filePath).GetAwaiter().GetResult();
            sw.Stop();

            _logger.LogInformation("✔ Completed {File} in {Elapsed:0.00}s",
                Path.GetFileName(filePath), sw.Elapsed.TotalSeconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "❌ Failed {File} after {Elapsed:0.00}s",
                Path.GetFileName(filePath), sw.Elapsed.TotalSeconds);
        }
        finally
        {
            _logger.LogInformation(new string('─', 60));
        }
    }

    /// <summary>
    /// Processes a single file, handling its parsing, validation, and storage operations.
    /// </summary>
    private async Task ProcessSingleFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Processing: {file}", filePath);

            var fileExtension = Path.GetExtension(filePath);
            var fileSize = new FileInfo(filePath).Length;
            var fileName = Path.GetFileName(filePath);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var processor = scope.ServiceProvider.GetRequiredService<IInvoiceProcessor>();
                var reservationFileService = scope.ServiceProvider.GetRequiredService<IReservationFileService>();
                var parser = _parserFactory.GetParser(fileExtension);

                var result = await processor.ProcessAsync(filePath, parser);

                if (result.IsSuccess)
                {
                    _logger.LogInformation("  • Processing succeeded: {Rows} rows", result.RowCount);
                    var reservationFile = ReservationFileModel.Success(result.InvoiceNumber, fileName, fileExtension, filePath, result.RowCount, fileSize);
                    await reservationFileService.CreateReservationFile(reservationFile);
                    _fileStorageService.MoveToProcessed(filePath, reservationFile.UniqueFileName);
                }
                else
                {
                    _logger.LogWarning("  • Processing failed: {Error}", result.ErrorMessage);
                    var reservationFile = ReservationFileModel.Fail(fileName, fileExtension, filePath, fileSize, result.ErrorMessage);
                    await reservationFileService.CreateReservationFile(reservationFile);
                    _fileStorageService.MoveToError(filePath, reservationFile.UniqueFileName);
                }
            }
        }
        catch (NotSupportedException ex)
        {
            var fileName = Path.GetFileName(filePath);
            _logger.LogWarning(ex, "Unsupported file type for {file}", filePath);
            _fileStorageService.MoveToError(filePath, $"{Guid.NewGuid().ToString()}{fileName}");
        }
        catch (Exception ex)
        {
            var fileName = Path.GetFileName(filePath);
            _logger.LogError(ex, "Unexpected error: {file}", filePath);
            _fileStorageService.MoveToError(filePath, $"{Guid.NewGuid().ToString()}{fileName}");
        }
    }
}