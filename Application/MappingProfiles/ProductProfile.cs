using Application.Model.Category;
using Application.Model.Product;
using Application.Model.Promotion;
using Application.Model.Supplier;
using AutoMapper;
using Core.Entities;
using Newtonsoft.Json;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Product -> ProductResponseDto (chỉ map 1 lần, xử lý Image trong đây luôn)
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.Image, opt => opt.MapFrom(
                src => string.IsNullOrEmpty(src.Image)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(src.Image)
            ));

        CreateMap<Product, ProductDto>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))  // ánh xạ Id
             .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))  // ánh xạ ProductName thành Name
             .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))  // ánh xạ Price
              .ForMember(dest => dest.Image, opt => opt.MapFrom(
                src => string.IsNullOrEmpty(src.Image)
                    ? new List<string>()
                    : JsonConvert.DeserializeObject<List<string>>(src.Image)))
             .ForMember(dest => dest.Promotion, opt => opt.MapFrom(src => src.Promotion));  // ánh xạ Promotion

        // Supplier ↔ SupplierDto
        CreateMap<Supplier, SupplierDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.SupplierName))
            .ReverseMap()
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Name));

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentName,
                       opt => opt.MapFrom(src => src.Parent != null ? src.Parent.CategoryName : null))
            .ForMember(dest => dest.ParentId,
                       opt => opt.MapFrom(src => src.ParentId));




        // Promotion <-> PromotionDto
        CreateMap<Promotion, PromotionDto>().ReverseMap()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ReverseMap();
    }
}
