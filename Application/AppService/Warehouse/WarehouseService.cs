using Application.IService;
using Application.Model.Warehouse;
using Core.Entities;

namespace Application.AppService.Warehouse;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _repository;

    public WarehouseService(IWarehouseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WarehouseResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var warehouses = await _repository.GetAllAsync(cancellationToken);
        return warehouses.Select(MapToResponse).ToList();
    }

    public async Task<WarehouseResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        return warehouse == null
            ? null
            : MapToResponse(warehouse);
    }

    public async Task<List<WarehouseResponse>> GetDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        var warehouses = await _repository.GetDeletedAsync(
            cancellationToken);

        return warehouses.Select(MapToResponse).ToList();
    }

    public async Task<WarehouseResponse> CreateAsync(
        CreateWarehouseRequest request,
        string username,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var warehouse = new Core.Entities.Warehouse
        {
            Id = Guid.NewGuid(),
            ImportDate = request.ImportDate,
            CreatedBy = username,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        foreach (var detailRequest in request.Details)
        {
            var detail = new WarehouseDetail
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse.Id,
                ProductId = detailRequest.ProductId,
                ProductName = detailRequest.ProductName.Trim(),
                SupplierName = detailRequest.SupplierName.Trim(),
                Quantity = detailRequest.Quantity,
                ImportPrice = detailRequest.ImportPrice,
                TotalPrice =
                    detailRequest.Quantity * detailRequest.ImportPrice
            };

            warehouse.Details.Add(detail);
        }

        warehouse.TotalAmount =
            warehouse.Details.Sum(x => x.TotalPrice);

        await _repository.AddAsync(
            warehouse,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(warehouse);
    }

    public async Task<WarehouseResponse?> UpdateAsync(
        Guid id,
        UpdateWarehouseRequest request,
        string username,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var warehouse = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (warehouse == null)
        {
            return null;
        }

        warehouse.ImportDate = request.ImportDate;
        warehouse.UpdatedBy = username;
        warehouse.UpdatedAt = DateTime.UtcNow;

        // Lấy danh sách detail cũ
        var oldDetails = warehouse.Details.ToList();

        // Xóa detail cũ khỏi database
        await _repository.RemoveDetailsAsync(
            oldDetails,
            cancellationToken);

        // Xóa detail cũ khỏi collection
        warehouse.Details.Clear();

        // Thêm detail mới
        foreach (var detailRequest in request.Details)
        {
            var detail = new WarehouseDetail
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse.Id,
                ProductId = detailRequest.ProductId,
                ProductName = detailRequest.ProductName.Trim(),
                SupplierName = detailRequest.SupplierName.Trim(),
                Quantity = detailRequest.Quantity,
                ImportPrice = detailRequest.ImportPrice,
                TotalPrice =
                    detailRequest.Quantity * detailRequest.ImportPrice
            };

            warehouse.Details.Add(detail);
        }

        // Tính lại tổng tiền nhập
        warehouse.TotalAmount =
            warehouse.Details.Sum(x => x.TotalPrice);

        _repository.Update(warehouse);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return MapToResponse(warehouse);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        string username,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (warehouse == null)
        {
            return false;
        }

        warehouse.IsDeleted = true;
        warehouse.DeletedBy = username;
        warehouse.DeletedAt = DateTime.UtcNow;

        _repository.Update(warehouse);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid id,
        string username,
        CancellationToken cancellationToken = default)
    {
        var warehouse = await _repository.GetDeletedByIdAsync(
            id,
            cancellationToken);

        if (warehouse == null)
        {
            return false;
        }

        warehouse.IsDeleted = false;
        warehouse.DeletedBy = null;
        warehouse.DeletedAt = null;
        warehouse.UpdatedBy = username;
        warehouse.UpdatedAt = DateTime.UtcNow;

        _repository.Update(warehouse);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static void ValidateRequest(
        CreateWarehouseRequest request)
    {
        if (request.Details == null ||
            request.Details.Count == 0)
        {
            throw new ArgumentException(
                "Phiếu nhập phải có ít nhất một sản phẩm.");
        }

        ValidateDetails(request.Details);
    }

    private static void ValidateRequest(
        UpdateWarehouseRequest request)
    {
        if (request.Details == null ||
            request.Details.Count == 0)
        {
            throw new ArgumentException(
                "Phiếu nhập phải có ít nhất một sản phẩm.");
        }

        ValidateDetails(request.Details);
    }

    private static void ValidateDetails(
        IEnumerable<CreateWarehouseDetailRequest> details)
    {
        foreach (var detail in details)
        {
            if (detail.ProductId <= 0)
            {
                throw new ArgumentException(
                    "ProductId phải lớn hơn 0.");
            }

            if (string.IsNullOrWhiteSpace(detail.ProductName))
            {
                throw new ArgumentException(
                    "Tên sản phẩm không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(detail.SupplierName))
            {
                throw new ArgumentException(
                    "Nhà cung cấp không được để trống.");
            }

            if (detail.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Số lượng phải lớn hơn 0.");
            }

            if (detail.ImportPrice < 0)
            {
                throw new ArgumentException(
                    "Giá nhập không được nhỏ hơn 0.");
            }
        }
    }

    private static WarehouseResponse MapToResponse(
        Core.Entities.Warehouse warehouse)
    {
        return new WarehouseResponse
        {
            Id = warehouse.Id,
            TotalAmount = warehouse.TotalAmount,
            ImportDate = warehouse.ImportDate,
            CreatedBy = warehouse.CreatedBy,
            CreatedAt = warehouse.CreatedAt,
            UpdatedBy = warehouse.UpdatedBy,
            UpdatedAt = warehouse.UpdatedAt,
            DeletedBy = warehouse.DeletedBy,
            DeletedAt = warehouse.DeletedAt,
            IsDeleted = warehouse.IsDeleted,

            Details = warehouse.Details
                .Select(detail => new WarehouseDetailResponse
                {
                    Id = detail.Id,
                    ProductId = detail.ProductId,
                    ProductName = detail.ProductName,
                    SupplierName = detail.SupplierName,
                    Quantity = detail.Quantity,
                    ImportPrice = detail.ImportPrice,
                    TotalPrice = detail.TotalPrice
                })
                .ToList()
        };
    }
}