using Application.Model.Category;
using Application.Model.Promotion;
using Application.Model.Supplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.Product
{
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public SupplierDto Supplier { get; set; }
        public CategoryDto Category { get; set; }
        public PromotionDto Promotion { get; set; }
        public List<string>? Image { get; set; }
        public TrangThaiSanPham ProductStatus { get; set; }
        public string Slug { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public List<string>? Image { get; set; }
        public string slug { get; set; }
        public PromotionDto? Promotion { get; set; }

        public decimal DiscountedPrice
        {
            get
            {
                if (Promotion != null && Promotion.DiscountPercent > 0)
                {
                    var discountRate = Promotion.DiscountPercent / 100m;
                    return Math.Round(Price * (1 - discountRate), 2);
                }

                return Price;
            }
        }
    }
}