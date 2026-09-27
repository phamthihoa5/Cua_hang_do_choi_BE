using System.ComponentModel;

namespace Core.Common;

public interface IAuditedEntity
{
    [DefaultValue(false)]
    public bool IsDeleted { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}