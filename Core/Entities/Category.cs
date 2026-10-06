using Core.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    [Table("Category")]
    public class Category :BaseEntity, IAuditedEntity
    {
        public string CategoryName { get; set; } = string.Empty;

        // FK tới chính nó
        public Guid? ParentId { get; set; }
        public Category? Parent { get; set; }
        public string? Image { get; set; }
        public ICollection<Category> Children { get; set; }
        public Guid? ProductId { get; set; }
        public ICollection<Product> Products { get; set; }

        public bool IsDeleted { get; set; }

        // Audit
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        // Slug nếu bạn muốn SEO
        public string Slug { get; set; }
    }

}
