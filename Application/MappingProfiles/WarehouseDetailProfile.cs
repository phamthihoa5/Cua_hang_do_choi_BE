using Application.Model.Warehouse;
using Application.Model.WarehouseDetail;
using AutoMapper;
using Core.Entities;

namespace Application.MappingProfiles
{
    public class WarehouseDetailProfile : Profile
    {
        public WarehouseDetailProfile()
        {
            // Warehouse->WarehouseResponseDto
            CreateMap<Warehouse, WarehouseResponseDto>()
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.WarehouseDetails));

            // WarehouseDetail -> WarehouseDetailResponseDto
            CreateMap<WarehouseDetail, WarehouseDetailResponseDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Product.Supplier.SupplierName))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.ImportPrice));

            // Request mapping
            CreateMap<WarehouseRequestDto, Warehouse>();
            CreateMap<WarehouseDetailRequestDto, WarehouseDetail>();
        }
    }
}
