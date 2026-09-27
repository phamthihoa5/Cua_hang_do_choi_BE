using Application.Model.Cart;
using Application.Model.Product;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;
using Newtonsoft.Json;
using System.Linq;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {

        // Map Cart -> ProductCartItemDto (Ánh xạ sản phẩm trong giỏ hàng)
        CreateMap<Cart, ProductCartItemDto>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))  // Ánh xạ thông tin sản phẩm từ Cart
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice))  // Lấy giá unit price từ Cart
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))  // Số lượng sản phẩm trong giỏ hàng
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));  // Tính tổng giá trị của sản phẩm

        // Map List<Cart> -> CartResponseDto (Ánh xạ danh sách Cart thành CartResponseDto)
        CreateMap<IEnumerable<Cart>, CartResponseDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Select(cart => cart)))  // Ánh xạ từng Cart thành ProductCartItemDto
            .ForMember(dest => dest.TotalQuantity, opt => opt.MapFrom(src => src.Sum(cart => cart.Quantity)))  // Tổng số lượng các sản phẩm trong giỏ
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Sum(cart => cart.Quantity * cart.UnitPrice)));  // Tổng tiền của giỏ hàng

        // Map CartRequestDto -> Cart (khi thêm giỏ hàng)
        CreateMap<CartRequestDto, Cart>()
            .ForMember(dest => dest.IdProduct, opt => opt.MapFrom(src => src.ProductId))  // Lấy ProductId từ CartRequestDto
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))  // Lấy Quantity từ CartRequestDto
            .ForMember(dest => dest.UnitPrice, opt => opt.Ignore());  // Để UnitPrice trong service xử lý (do giá có thể thay đổi tùy theo tình huống)
    }
}
