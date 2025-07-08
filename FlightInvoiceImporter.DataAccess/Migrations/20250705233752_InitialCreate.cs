using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FlightInvoiceImporter.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "odeon");

            migrationBuilder.CreateTable(
                name: "reservation_files",
                schema: "odeon",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    unique_file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    file_size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    invoice_number = table.Column<long>(type: "bigint", nullable: false),
                    row_count = table.Column<int>(type: "integer", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_success = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservation_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "odeon",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<string>(type: "text", nullable: false),
                    customer = table.Column<string>(type: "text", nullable: false),
                    carrier_code = table.Column<string>(type: "text", nullable: false),
                    flight_number = table.Column<int>(type: "integer", nullable: false),
                    flight_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origin = table.Column<string>(type: "text", nullable: false),
                    destination = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    invoice_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reservations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reservation_carrier_flight_date",
                schema: "odeon",
                table: "reservations",
                columns: new[] { "carrier_code", "flight_number", "flight_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservation_files",
                schema: "odeon");

            migrationBuilder.DropTable(
                name: "reservations",
                schema: "odeon");
        }
    }
}
