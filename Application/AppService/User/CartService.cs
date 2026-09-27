using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Application.Model.Cart;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Services.ClaimService;

namespace Application.AppService.User
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public CartService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Helper: attach KM active vào cartDto
        /// </summary>
        private async Task AttachPromotionsAsync(CartResponseDto cartDto)
        {
            if (cartDto == null || !cartDto.Products.Any())
                return;

            var activePromotions = await _unitOfWork.promotionRepo.GetAll()
                .Include(p => p.Products)
                .Where(p => !p.IsDeleted && p.IsApproved
                            && p.StartDate <= DateTime.UtcNow
                            && p.EndDate >= DateTime.UtcNow)
                .ToListAsync();

            foreach (var item in cartDto.Products)
            {
                var promo = activePromotions
                    .Where(p => p.Products.Any(pr => pr.Id == item.Product.Id))
                    .OrderByDescending(p => p.DiscountPercent)
                    .FirstOrDefault();

                if (promo != null)
                {
                    item.Product.Promotion = new PromotionDto
                    {
                        Title = promo.Title,
                        DiscountPercent = promo.DiscountPercent ?? 0
                    };
                }
            }
        }

        public async Task<CartResponseDto> CreateCartAsync(CartRequestDto createCart)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var product = await _unitOfWork.productRepo
                .GetById(createCart.ProductId)
                .FirstOrDefaultAsync();

            if (product == null || product.IsDeleted)
                throw new BadRequestException("Sản phẩm không tồn tại.");

            var existing = await _unitOfWork.cartRepo
                .GetByUserAndProduct(userId, createCart.ProductId)
                .Include(c => c.Product)
                .FirstOrDefaultAsync();

            int currentQty = existing?.Quantity ?? 0;
            int totalRequested = currentQty + createCart.Quantity;

            if (totalRequested > product.Quantity)
                throw new BadRequestException($"Chỉ có thể thêm tối đa {product.Quantity - currentQty} sản phẩm.");

            // Tìm KM tốt nhất cho sản phẩm này
            var promotion = await _unitOfWork.promotionRepo.GetAll()
                .Where(p => !p.IsDeleted && p.IsApproved
                            && p.StartDate <= DateTime.UtcNow
                            && p.EndDate >= DateTime.UtcNow
                            && p.Products.Any(pr => pr.Id == product.Id))
                .OrderByDescending(p => p.DiscountPercent)
                .FirstOrDefaultAsync();

            decimal snapshotPrice = product.Price;
            if (promotion != null && promotion.DiscountPercent.HasValue)
            {
                snapshotPrice -= snapshotPrice * promotion.DiscountPercent.Value / 100;
            }

            if (existing != null)
            {
                existing.Quantity = totalRequested;
                existing.UnitPrice = snapshotPrice;
                await _unitOfWork.cartRepo.Update(existing);
            }
            else
            {
                var cart = _mapper.Map<Cart>(createCart);
                cart.Id = Guid.NewGuid();
                cart.IdUser = userId;
                cart.UnitPrice = snapshotPrice;
                cart.IsDeleted = false;

                await _unitOfWork.cartRepo.Add(cart);
            }

            await _unitOfWork.CompleteAsync();

            var cartItems = await _unitOfWork.cartRepo
                .GetByIdUser(userId, false)
                .Include(c => c.Product)
                .ToListAsync();

            var cartDto = _mapper.Map<CartResponseDto>(cartItems);
            await AttachPromotionsAsync(cartDto);

            return cartDto;
        }

        public async Task<CartResponseDto> UpdateCart(CartRequestDto updateCart)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var cartItem = await _unitOfWork.cartRepo
                .GetByUserAndProduct(userId, updateCart.ProductId)
                .Include(c => c.Product)
                .FirstOrDefaultAsync();

            if (cartItem == null || cartItem.IsDeleted)
                throw new BadRequestException("Không tìm thấy sản phẩm trong giỏ hàng.");

            var product = await _unitOfWork.productRepo
                .GetById(updateCart.ProductId)
                .FirstOrDefaultAsync();

            if (product == null || product.IsDeleted)
                throw new BadRequestException("Sản phẩm không tồn tại.");

            if (updateCart.Quantity > product.Quantity)
                throw new BadRequestException($"Chỉ còn {product.Quantity} sản phẩm trong kho.");

            cartItem.Quantity = updateCart.Quantity;
            // Giữ nguyên snapshot UnitPrice
            await _unitOfWork.cartRepo.Update(cartItem);
            await _unitOfWork.CompleteAsync();

            var cartItems = await _unitOfWork.cartRepo
                .GetByIdUser(userId, false)
                .Include(c => c.Product)
                .ToListAsync();

            var cartDto = _mapper.Map<CartResponseDto>(cartItems);
            await AttachPromotionsAsync(cartDto);

            return cartDto;
        }

        public async Task<string> DeleteCartAsync(Guid productId)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var cartItem = await _unitOfWork.cartRepo
                .GetByUserAndProduct(userId, productId)
                .Include(c => c.Product)
                .FirstOrDefaultAsync();

            if (cartItem == null || cartItem.IsDeleted)
                return "Không tìm thấy sản phẩm trong giỏ hàng.";

            cartItem.IsDeleted = true;
            await _unitOfWork.cartRepo.Update(cartItem);
            await _unitOfWork.CompleteAsync();

            return "Đã xóa sản phẩm khỏi giỏ hàng.";
        }

        public async Task<CartResponseDto> GetByIdAsync()
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var cartItems = await _unitOfWork.cartRepo
                .GetByIdUser(userId, false)
                .Include(c => c.Product)
                .ToListAsync();

            var cartDto = _mapper.Map<CartResponseDto>(cartItems);
            await AttachPromotionsAsync(cartDto);

            return cartDto;
        }

        public async Task<CartResponseDto> GetAllAsync()
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var items = await _unitOfWork.cartRepo
                        .GetByIdUser(userId, false)
                        .Include(c => c.Product)
                        .ToListAsync();

            if (!items.Any()) return null;

            var cartDto = _mapper.Map<CartResponseDto>(items);
            await AttachPromotionsAsync(cartDto);

            return cartDto;
        }
    }
}
