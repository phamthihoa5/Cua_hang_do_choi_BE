using Application.Model.Order;
using Application.Model.OrderDetail;
using Application.Model.Product;
using AutoMapper;
using Core.Entities;

namespace Application.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Order -> OrderResponseDto
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.TotalPrice,
                    opt => opt.MapFrom(src => src.OrderDetail.Sum(od => od.Quantity * od.Price) + src.ShippingFee))
                .ForMember(dest => dest.OrderDetails,
                    opt => opt.MapFrom(src => src.OrderDetail));

            // OrderDetails -> OrderDetailResponseDto
            CreateMap<OrderDetails, OrderDetailResponseDto>()
                .ForMember(dest => dest.Product,
                    opt => opt.MapFrom(src => new ProductDto
                    {
                        Id = src.IdProduct,
                        Name = src.Product.ProductName,
                        Price = src.Product.Price, // giá lúc mua (ko phụ thuộc giá sp hiện tại)
                        slug = src.Product.Slug
                    }))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            // Order -> ListOrderDto (group theo user, Orders sẽ build ở service)
            CreateMap<Order, ListOrderDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.IdUser))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.UserName))
                .ForMember(dest => dest.Orders, opt => opt.Ignore());

            CreateMap<OrderRequestDto, Order>();
        }
    }
}
