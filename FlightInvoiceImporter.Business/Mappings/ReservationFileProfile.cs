using AutoMapper;
using FlightInvoiceImporter.DataAccess.Entities;
using FlightInvoiceImporter.Models.ReservationFile;

namespace FlightInvoiceImporter.Business.Mappings;

public class ReservationFileProfile : Profile
{
    public ReservationFileProfile()
    {
        CreateMap<ReservationFileModel, ReservationFileEntity>();
        CreateMap<ReservationFileEntity, ReservationFileModel>();

    }
}