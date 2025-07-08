using FlightInvoiceImporter.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlightInvoiceImporter.DataAccess;

public class ReservationDbContext : DbContext
{
    private readonly IConfiguration _configuration;


    public ReservationDbContext(DbContextOptions options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();
    public DbSet<ReservationFileEntity> ReservationFiles => Set<ReservationFileEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseNpgsql(connectionString,
            builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(2), null);
                builder.MigrationsHistoryTable("__EFMigrationsHistory", "odeon");
            });
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("odeon");
        builder.Entity<ReservationEntity>(entity =>
        {
            entity.HasIndex(e => new { e.CarrierCode, e.FlightNumber, e.FlightDate })
                .HasDatabaseName("ix_reservation_carrier_flight_date");
        });
    }
}