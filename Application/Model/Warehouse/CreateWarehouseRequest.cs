namespace Application.Model.Warehouse;

public class CreateWarehouseRequest
{
    public DateTime ImportDate { get; set; }

    public List<CreateWarehouseDetailRequest> Details { get; set; } = new();
}

public class CreateWarehouseDetailRequest
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string SupplierName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }
}
