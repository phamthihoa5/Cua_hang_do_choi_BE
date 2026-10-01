using Application.Model.Product;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Promotion
{
    public class PromotionRequestDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? DiscountPercent { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
        public List<Guid> ProductIds { get; set; } = new();
    }
}
