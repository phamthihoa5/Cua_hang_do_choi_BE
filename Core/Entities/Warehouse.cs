namespace Core.Entities;

public class Warehouse
{
    public Guid Id { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime ImportDate { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<WarehouseDetail> Details { get; set; }
        = new List<WarehouseDetail>();
}
