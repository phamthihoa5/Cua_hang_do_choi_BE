using Application.Model.Product;

namespace Application.Model.Cart
{
    public class CartResponseDto
    {
        public ICollection<ProductCartItemDto> Products { get; set; } = new List<ProductCartItemDto>();

        public int TotalQuantity => Products.Sum(p => p.Quantity);
        public decimal TotalPrice => Products.Sum(p => p.TotalPrice);
    }

    public class ProductCartItemDto
    {
        public ProductDto Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }   // giá snapshot lúc thêm giỏ
        public decimal TotalPrice => Product.DiscountedPrice * Quantity;
    }
}
