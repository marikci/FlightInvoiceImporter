using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlightInvoiceImporter.DataAccess.Entities;

[Table("reservations")]
public class ReservationEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Column("booking_id")] 
    public string BookingId { get; set; }

    [Column("customer")] 
    public string Customer { get; set; }

    [Column("carrier_code")] 
    public string CarrierCode { get; set; }

    [Column("flight_number")] 
    public int FlightNumber { get; set; }

    [Column("flight_date")] 
    public DateTime FlightDate { get; set; }

    [Column("origin")] 
    public string Origin { get; set; }

    [Column("destination")] 
    public string Destination { get; set; }

    [Column("price")] 
    public decimal Price { get; set; }

    [Column("invoice_number")] 
    public long? InvoiceNumber { get; set; }
}