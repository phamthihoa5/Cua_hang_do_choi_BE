using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.New
{
    public class NewsResponse
    {
        public Guid Id { get; set; }
        public string? Image { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Slug { get; set; }
        public Guid? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public bool IsDeleted { get; set; }

        public string CreatedbyStr { get; set; } = "system";
    }
}
