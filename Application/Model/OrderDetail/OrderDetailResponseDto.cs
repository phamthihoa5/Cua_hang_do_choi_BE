using Application.Model.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.OrderDetail
{
    public class OrderDetailResponseDto
    {
        public ProductDto Product { get; set; } = new ProductDto();
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Product.DiscountedPrice;
    }

    public class OrderDetailDto
    {
        public ICollection<OrderDetailResponseDto> orderDetails { get; set; }
        public decimal GrandTotal => orderDetails?.Sum(d => d.TotalPrice) ?? 0;
    }
}
