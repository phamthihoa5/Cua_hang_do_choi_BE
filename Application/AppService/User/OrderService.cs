using Application.Common.Models;
using Application.Exceptions;
using Application.Interface;
using Application.IService;
using Application.IService.User;
using Application.Model.API;
using Application.Model.Order;
using Application.Model.OrderDetail;
using Application.Model.Promotion;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using static Core.Entities.Enum;

namespace Application.AppService.User
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGhnService _ghnService;
        private readonly IOrderNotifier? _orderNotifier;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, ITokenService tokenService, UserManager<ApplicationUser> userManager, IGhnService ghnService, IOrderNotifier? orderNotifier = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _tokenService = tokenService;
            _userManager = userManager;
            _ghnService = ghnService;
            _orderNotifier = orderNotifier;
        }

        // =======================================================
        // 🟢 Hàm hỗ trợ: Gắn khuyến mãi cho từng sản phẩm trong đơn
        // =======================================================
        private async Task AttachPromotionsAsync(IEnumerable<OrderDetailResponseDto> orderDetails)
        {
            if (orderDetails == null || !orderDetails.Any())
                return;

            var productIds = orderDetails.Select(od => od.Product?.Id).Where(id => id != null).ToList();

            var activePromotions = await _unitOfWork.promotionRepo.GetAll()
                .Include(p => p.Products)
                .Where(p => !p.IsDeleted && p.IsApproved &&
                            p.StartDate <= DateTime.UtcNow &&
                            p.EndDate >= DateTime.UtcNow &&
                            p.Products.Any(pr => productIds.Contains(pr.Id)))
                .ToListAsync();

            foreach (var detail in orderDetails)
            {
                var promo = activePromotions
                    .FirstOrDefault(p => p.Products.Any(pr => pr.Id == detail.Product.Id));

                if (promo != null)
                {
                    detail.Product.Promotion = new PromotionDto
                    {
                        Title = promo.Title,
                        DiscountPercent = promo.DiscountPercent ?? 0
                    };
                }
            }
        }

        // =======================================================
        // 🟢 Tạo đơn hàng mới
        // =======================================================
        public async Task<OrderResponseDto> CreateOrderAsync(OrderRequestDto createOrder)
        {
            // 🔐 XÁC THỰC NGƯỜI DÙNG
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var userRoles = await _userManager.GetRolesAsync(user);
            var isStaff = userRoles.Contains(RoleName.Staff) || userRoles.Contains(RoleName.Manager);

            // 🎯 Đặt trạng thái đơn hàng tùy vai trò
            var orderStatus = isStaff
                ? TrangThaiDonHang.Đã_giao_hàng
                : TrangThaiDonHang.Chờ_xử_lý;

            // 🛒 LẤY DANH SÁCH SẢN PHẨM TRONG ĐƠN
            List<Cart> cartItems;

            if (isStaff)
            {
                // ✅ Nếu là nhân viên, tạo cart tạm từ request
                if (createOrder.Products == null || !createOrder.Products.Any())
                    throw new BadRequestException("Danh sách sản phẩm trống.");

                cartItems = new List<Cart>();

                foreach (var p in createOrder.Products)
                {
                    var product = await _unitOfWork.productRepo.GetById(p.ProductId)
                        .Include(x => x.Supplier)
                        .Include(x => x.Category)
                        .FirstOrDefaultAsync();

                    if (product == null)
                        throw new BadRequestException($"Không tìm thấy sản phẩm ID {p.ProductId}");

                    cartItems.Add(new Cart
                    {
                        Id = Guid.NewGuid(),
                        IdUser = userId,
                        IdProduct = product.Id,
                        Quantity = p.Quantity,
                        UnitPrice = product.Price,
                        Product = product
                    });
                }
            }
            else
            {
                // 👤 Nếu là khách hàng: lấy giỏ hàng thực tế
                cartItems = await _unitOfWork.cartRepo.GetByIdUser(userId, false)
                    .Include(c => c.Product)
                    .ToListAsync();

                if (!cartItems.Any())
                    throw new BadRequestException("Giỏ hàng trống.");
            }

            // 📦 KIỂM TRA TỒN KHO TRƯỚC KHI TẠO ĐƠN
            foreach (var item in cartItems)
            {
                var availableQty = await _unitOfWork.warehouseDetailRepo.GetAll()
                    .Where(d => d.ProductId == item.IdProduct && !d.IsDeleted && d.Quantity > 0)
                    .SumAsync(d => (int?)d.Quantity) ?? 0;

                if (availableQty < item.Quantity)
                    throw new BadRequestException($"Sản phẩm {item.Product.ProductName} không đủ tồn kho.");
            }

            // 🧾 TẠO ĐƠN HÀNG MỚI
            var order = new Order
            {
                Id = Guid.NewGuid(),
                IdUser = userId,
                Address = createOrder.Address,
                Phone = createOrder.Phone,
                ProvinceId = createOrder.ProvinceId,
                DistrictId = createOrder.DistrictId,
                WardCode = createOrder.WardCode,
                ShippingFee = createOrder.ShippingFee ?? 0,
                OrderDate = DateTime.UtcNow,
                OrderStatus = orderStatus,
                IsDeleted = false
            };

            await _unitOfWork.orderRepo.Add(order);

            decimal total = 0;

            // 🔁 DUYỆT TỪNG SẢN PHẨM TRONG CART
            foreach (var item in cartItems)
            {
                var remainingQty = item.Quantity;
                var product = item.Product;
                var price = item.UnitPrice;

                // 🏭 Lấy chi tiết kho có hàng (FIFO)
                var warehouseDetails = await _unitOfWork.warehouseDetailRepo.GetAll()
                    .Include(d => d.Warehouse)
                    .Where(d => d.ProductId == item.IdProduct && !d.IsDeleted && d.Quantity > 0)
                    .OrderBy(d => d.Warehouse.DateEntered)
                    .ToListAsync();

                foreach (var detail in warehouseDetails)
                {
                    if (remainingQty <= 0)
                        break;

                    int takeQty = Math.Min(remainingQty, detail.Quantity);

                    // ✅ Trừ tồn kho chi tiết
                    detail.Quantity -= takeQty;
                    detail.UpdatedOn = DateTime.UtcNow;
                    await _unitOfWork.warehouseDetailRepo.Update(detail);

                    // 🧾 Tạo OrderDetail tương ứng
                    var orderDetail = new OrderDetails
                    {
                        Id = Guid.NewGuid(),
                        IdOrder = order.Id,
                        IdProduct = product.Id,
                        Quantity = takeQty,
                        Price = price,
                        IsDeleted = false
                    };
                    await _unitOfWork.orderDetailsRepo.Add(orderDetail);

                    total += takeQty * price;
                    remainingQty -= takeQty;
                }

                // ⚠️ Nếu qua hết kho vẫn thiếu hàng => lỗi dữ liệu
                if (remainingQty > 0)
                    throw new BadRequestException($"Kho hàng bị thiếu dữ liệu FIFO cho sản phẩm {product.ProductName}.");

                // 🔻 Cập nhật tổng tồn sản phẩm
                product.Quantity -= item.Quantity;
                product.ProductStatus = product.Quantity > 0
                    ? TrangThaiSanPham.ConHang
                    : TrangThaiSanPham.HetHang;
                await _unitOfWork.productRepo.Update(product);

                // 🧹 Nếu là khách hàng thì xóa sản phẩm khỏi giỏ
                if (!isStaff)
                    await _unitOfWork.cartRepo.Delete(item);
            }

            // 💰 Ghi tổng tiền đơn hàng (tiền hàng + phí vận chuyển)
            order.TotalPrice = total + order.ShippingFee;
            await _unitOfWork.CompleteAsync();

            // 🚚 TẠO VẬN ĐƠN TRÊN GHN (nếu chọn Tỉnh/Huyện/Xã)
            if (createOrder.DistrictId.HasValue && !string.IsNullOrEmpty(createOrder.WardCode))
            {
                try
                {
                    var orderFullForGhn = await _unitOfWork.orderRepo.GetFullById(order.Id).FirstOrDefaultAsync();
                    var recipientName = !string.IsNullOrEmpty(createOrder.RecipientName)
                        ? createOrder.RecipientName
                        : (user?.UserName ?? "Khach hang");

                    var ghnRes = await _ghnService.CreateShippingOrderAsync(
                        orderFullForGhn ?? order,
                        createOrder.DistrictId.Value,
                        createOrder.WardCode,
                        createOrder.Address,
                        recipientName,
                        createOrder.Phone ?? "0987654321",
                        0
                    );

                    if (ghnRes != null && !string.IsNullOrEmpty(ghnRes.OrderCode))
                    {
                        order.GhnOrderCode = ghnRes.OrderCode;
                        await _unitOfWork.orderRepo.Update(order);
                        await _unitOfWork.CompleteAsync();
                        Console.WriteLine($"📦 Created GHN OrderCode '{ghnRes.OrderCode}' for Order {order.Id}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to create GHN order: {ex.Message}");
                }
            }

            // 📄 LẤY LẠI ĐƠN ĐẦY ĐỦ & MAP DTO
            var orderFull = await _unitOfWork.orderRepo.GetFullById(order.Id).FirstOrDefaultAsync();
            var orderDto = _mapper.Map<OrderResponseDto>(orderFull);

            // 🎁 Gắn thêm khuyến mãi cho các sản phẩm trong đơn
            await AttachPromotionsAsync(orderDto.OrderDetails);

            return orderDto;
        }



        public async Task<OrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderDto updateOrder)
        {
            var order = await _unitOfWork.orderRepo.GetById(orderId)
                .Include(o => o.User)
                .FirstOrDefaultAsync();

            if (order == null)
                throw new BadRequestException("Không tìm thấy đơn hàng.");

            if ((int)updateOrder.OrderStatus < (int)order.OrderStatus)
                throw new BadRequestException("Không thể quay lại trạng thái trước.");

            order.OrderStatus = updateOrder.OrderStatus;
            await _unitOfWork.orderRepo.Update(order);
            await _unitOfWork.CompleteAsync();

            var orderDto = _mapper.Map<OrderResponseDto>(order);
            await AttachPromotionsAsync(orderDto.OrderDetails);
            return orderDto;
        }

        public async Task<string> DeleteOrderAsync(Guid orderId)
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var order = await _unitOfWork.orderRepo.GetById(orderId)
                .Include(o => o.OrderDetail)
                .FirstOrDefaultAsync();

            if (order == null)
                throw new BadRequestException("Không tìm thấy đơn hàng.");

            if (order.IdUser != userId)
                throw new BadRequestException("Bạn không có quyền hủy đơn hàng này.");

            var ghnStatusLower = order.GhnStatus?.ToLower()?.Trim() ?? "";
            bool isShipped = ghnStatusLower == "picked" ||
                             ghnStatusLower == "storing" ||
                             ghnStatusLower == "transporting" ||
                             ghnStatusLower == "sorting" ||
                             ghnStatusLower == "delivering" ||
                             ghnStatusLower == "delivered";

            bool canCancel = order.OrderStatus == TrangThaiDonHang.Chờ_xử_lý ||
                            (order.OrderStatus == TrangThaiDonHang.Đã_xác_nhận && !isShipped);

            if (!canCancel)
                throw new BadRequestException("Đơn hàng đã được bên vận chuyển tiếp nhận/giao hàng, không thể hủy.");

            order.IsDeleted = true;
            order.OrderStatus = TrangThaiDonHang.Đã_hủy;
            order.GhnStatus = "cancel";

            foreach (var detail in order.OrderDetail)
            {
                var product = await _unitOfWork.productRepo.GetById(detail.IdProduct).FirstOrDefaultAsync();
                if (product == null) continue;

                product.Quantity += detail.Quantity;
                await _unitOfWork.productRepo.Update(product);

                // Hoàn lại hàng vào WarehouseDetail
                var oldLot = await _unitOfWork.warehouseDetailRepo.GetAll()
                    .Where(d => d.ProductId == detail.IdProduct && !d.IsDeleted)
                    .OrderBy(d => d.Warehouse.DateEntered)
                    .FirstOrDefaultAsync();

                if (oldLot != null)
                {
                    oldLot.Quantity += detail.Quantity;
                    oldLot.UpdatedOn = DateTime.UtcNow;
                    await _unitOfWork.warehouseDetailRepo.Update(oldLot);
                }
                else
                {
                    var newLot = new WarehouseDetail
                    {
                        ProductId = detail.IdProduct,
                        Quantity = detail.Quantity,
                        ImportPrice = detail.Price,
                        IsDeleted = false
                    };
                    await _unitOfWork.warehouseDetailRepo.Add(newLot);
                }
            }

            await _unitOfWork.orderRepo.Update(order);
            await _unitOfWork.CompleteAsync();

            if (_orderNotifier != null)
            {
                await _orderNotifier.NotifyOrderStatusChangedAsync(order.Id, order.GhnOrderCode, (int)order.OrderStatus, order.OrderStatus.ToString(), order.GhnStatus);
            }

            return "Đã hủy đơn hàng và hoàn kho thành công.";
        }


        public async Task<ListOrderDto?> GetOrderByIdUserAsync()
        {
            var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
            if (!Guid.TryParse(userIdStr, out var userId))
                throw new BadRequestException("invalid_user");

            var orders = await _unitOfWork.orderRepo
                .GetFullByUserId(userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            if (!orders.Any()) return null;

            var mapped = _mapper.Map<List<OrderResponseDto>>(orders);
            foreach (var dto in mapped)
                await AttachPromotionsAsync(dto.OrderDetails);

            var userName = orders.First().User?.UserName ?? "Unknown";

            return new ListOrderDto
            {
                UserId = userId,
                Name = userName,
                Orders = mapped
            };
        }



        public async Task<PageList<OrderResponseDto>> GetAllOrdersAsync(QueryParam? query = null)
        {
            var source = _unitOfWork.orderRepo.GetAll()
                .Include(o => o.User)
                .Include(o => o.OrderDetail)
                    .ThenInclude(od => od.Product)
                .AsNoTracking()
                .AsQueryable();

            // 🔍 Tìm kiếm theo tên user hoặc địa chỉ
            if (!string.IsNullOrEmpty(query?.Search))
            {
                var search = query.Search.ToLower();
                source = source.Where(o =>
                    o.User.UserName.ToLower().Contains(search) ||
                    o.Address.ToLower().Contains(search));
            }

            source = source.OrderByDescending(o => o.OrderDate);

            var total = await source.CountAsync();
            var orders = await source
                .Skip(((query?.PageNumber ?? 1) - 1) * (query?.PageSize ?? 10))
                .Take(query?.PageSize ?? 10)
                .ToListAsync();

            if (!orders.Any())
                return new PageList<OrderResponseDto>(new List<OrderResponseDto>(), 0, query?.PageNumber ?? 1, query?.PageSize ?? 10);

            var mapped = _mapper.Map<List<OrderResponseDto>>(orders);

            // Gắn thông tin khuyến mãi vào từng order detail (nếu có)
            foreach (var dto in mapped)
                await AttachPromotionsAsync(dto.OrderDetails);

            return new PageList<OrderResponseDto>(
                mapped,
                total,
                query?.PageNumber ?? 1,
                query?.PageSize ?? 10
            );
        }



    }
}
