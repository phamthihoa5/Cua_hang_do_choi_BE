using Microsoft.AspNetCore.Http;

public class ProductRequestDto
{
    public string ProductName { get; set; }
    public Guid IdCategory { get; set; }
    public Guid IdSupplier { get; set; }
    public Guid? IdPromotion { get; set; }
    public List<IFormFile>? Images { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
