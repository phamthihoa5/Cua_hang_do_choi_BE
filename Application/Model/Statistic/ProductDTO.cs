using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Entities.Enum;

namespace Application.Model.Statistic
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public SupplierDto Supplier { get; set; }
        public CategoryDto Category { get; set; }
        public PromotionDto Promotion { get; set; }
        public string Image { get; set; } 
        public TrangThaiSanPham ProductStatus { get; set; }
        public string Slug { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class SupplierDto
    {
        public string Name { get; set; }
    }

    public class CategoryDto
    {
        public string ParentName { get; set; }
        public string ChildName { get; set; }
    }

    public class PromotionDto
    {
        public string Name { get; set; }
    }

}
