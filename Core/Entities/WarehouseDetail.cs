namespace Core.Entities;

public class WarehouseDetail
{
    public Guid Id { get; set; }

    public Guid WarehouseId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}
