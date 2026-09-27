using Application.Model.Product;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;

namespace Application.MapperProfiles
{
    public class PromotionProfile : Profile
    {
        public PromotionProfile()
        {
            // Map Promotion -> PromotionResponseDto
            CreateMap<Promotion, PromotionResponseDto>()
             .ForMember(dest => dest.Products,
                 opt => opt.MapFrom((src, dest, destMember, context) =>
                     src.Products.Select(p => new ProductDto
                     {
                         Id = p.Id,
                         Name = p.ProductName,
                         Price = p.Price,
                         Promotion = new PromotionDto
                         {
                             Title = src.Title,
                             DiscountPercent = src.DiscountPercent ?? 0
                         }
                     }).ToList()
                 ));


            // Map PromotionRequestDto -> Promotion
            CreateMap<PromotionRequestDto, Promotion>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore()) // sẽ handle ở service
                .ForMember(dest => dest.IsApproved, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedBy, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore());
        }
    }
}
