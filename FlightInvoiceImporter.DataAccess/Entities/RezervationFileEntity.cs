using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightInvoiceImporter.DataAccess.Entities;

[Table("reservation_files")]
public class ReservationFileEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("file_name")]
    public string FileName { get; set; }

    [Column("unique_file_name")]
    public string UniqueFileName { get; set; }

    [Column("file_path")]
    public string FilePath { get; set; }

    [Column("file_size_in_bytes")]
    public long FileSizeInBytes { get; set; }

    [Column("invoice_number")]
    public long InvoiceNumber { get; set; }

    [Column("row_count")]
    public int RowCount { get; set; }

    [Column("processed_at")]
    public DateTime ProcessedAt { get; set; }

    [Column("is_success")]
    public bool IsSuccess { get; set; }

    [Column("error_message")]
    public string? ErrorMessage { get; set; }
}