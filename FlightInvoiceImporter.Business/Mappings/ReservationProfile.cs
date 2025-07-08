using AutoMapper;
using FlightInvoiceImporter.DataAccess.Entities;
using FlightInvoiceImporter.Models.Reservation;

namespace FlightInvoiceImporter.Business.Mappings;

public class ReservationProfile : Profile
{
    public ReservationProfile()
    {
        CreateMap<ReservationModel, ReservationEntity>();
        CreateMap<ReservationEntity, ReservationModel>();

    }
}
