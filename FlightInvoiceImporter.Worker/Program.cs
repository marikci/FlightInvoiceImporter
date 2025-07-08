using FlightInvoiceImporter.Business.Interfaces;
using FlightInvoiceImporter.Business.Interfaces.Parser;
using FlightInvoiceImporter.Business.Mappings;
using FlightInvoiceImporter.Business.Services;
using FlightInvoiceImporter.Business.Services.Parser;
using FlightInvoiceImporter.DataAccess;
using FlightInvoiceImporter.DataAccess.Repositories;
using FlightInvoiceImporter.DataAccess.Repositories.Interfaces;
using FlightInvoiceImporter.Models.Config;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using Serilog;

namespace FlightInvoiceImporter.Worker;

public class Program
{
    public static int Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        //Logger
        builder.Logging.ClearProviders();
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();
        builder.Logging.AddSerilog();

        try
        {
            //Razor
            var templatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates");
            var engine = new RazorLightEngineBuilder()
                .UseFileSystemProject(templatesRoot)
                .UseMemoryCachingProvider()
                .Build();
            builder.Services.AddSingleton<IRazorLightEngine>(engine);

            builder.Services.AddHostedService<InvoiceImportWorker>();
            builder.Services.Configure<FileStorageOptions>(builder.Configuration.GetSection("FileStorage"));
            builder.Services.Configure<MailOptions>(builder.Configuration.GetSection("MailSettings"));
            builder.Services.Configure<InvoiceReportOptions>(builder.Configuration.GetSection("InvoiceReport"));
            builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
            builder.Services.AddScoped<IInvoiceProcessor, InvoiceProcessor>();
            builder.Services.AddScoped<IReservationFileService, ReservationFileService>();
            builder.Services.AddScoped<IReservationService, ReservationService>();
            builder.Services.AddScoped<IMailService, MailService>();
            builder.Services.AddSingleton<IInvoiceParserFactory, InvoiceParserFactory>();
            builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
            builder.Services.AddScoped<IReservationFileRepository, ReservationFileRepository>();
            builder.Services.AddSingleton<PdfInvoiceParser>();

            builder.Services.AddDbContext<ReservationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            builder.Services.AddAutoMapper(typeof(Program));
            builder.Services.AddAutoMapper(typeof(ReservationFileProfile));
            builder.Services.AddAutoMapper(typeof(ReservationProfile));
 

            var host = builder.Build();
            using (var scope = host.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
                dbContext.Database.Migrate();
            }

            host.Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "FlightInvoiceImporter could not start.");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}