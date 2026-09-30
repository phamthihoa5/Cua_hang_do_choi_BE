using Application.Model.Warehouse;

namespace Application.IService;

public interface IWarehouseService
{
    Task<List<WarehouseResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<WarehouseResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<WarehouseResponse>> GetDeletedAsync(
        CancellationToken cancellationToken = default);

    Task<WarehouseResponse> CreateAsync(
        CreateWarehouseRequest request,
        string username,
        CancellationToken cancellationToken = default);

    Task<WarehouseResponse?> UpdateAsync(
        Guid id,
        UpdateWarehouseRequest request,
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        string username,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(
        Guid id,
        string username,
        CancellationToken cancellationToken = default);
}
