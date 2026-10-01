using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Supplier
{
    public class SupplierResponseDto
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Note { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CreatedbyStr { get; set; }
        public string? UpdatedbyStr { get; set; } = string.Empty;
    }

    public class SupplierDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
