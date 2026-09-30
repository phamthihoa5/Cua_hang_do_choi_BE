namespace Application.Model.Warehouse;

public class UpdateWarehouseRequest
{
    public DateTime ImportDate { get; set; }

    public List<CreateWarehouseDetailRequest> Details { get; set; } = new();
}
