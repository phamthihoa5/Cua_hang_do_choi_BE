namespace Application.Model.Warehouse;

public class WarehouseResponse
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

    public List<WarehouseDetailResponse> Details { get; set; } = new();
}

public class WarehouseDetailResponse
{
    public Guid Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public decimal TotalPrice { get; set; }
}
