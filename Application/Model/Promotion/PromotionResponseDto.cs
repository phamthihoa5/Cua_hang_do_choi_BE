using Application.Model.Product;
using Application.Model.Statistic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Promotion
{
    public class PromotionResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public Guid? ApprovedBy { get; set; }
        public bool IsApproved { get; set; }
        public ICollection<ProductDto> Products { get; set; }
        public string Slug { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class PromotionDto
    {
        public string Title { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}