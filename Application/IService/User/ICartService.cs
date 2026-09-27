using Application.Model.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface ICartService
    {
        Task<CartResponseDto> CreateCartAsync(CartRequestDto createCart);
        Task<string> DeleteCartAsync(Guid productId); // chỉ cần productId vì user lấy từ JWT
        Task<CartResponseDto> UpdateCart(CartRequestDto updateCart); // không cần userId nữa
        Task<CartResponseDto> GetByIdAsync();
        Task<CartResponseDto> GetAllAsync(); // không cần userId
    }
}
