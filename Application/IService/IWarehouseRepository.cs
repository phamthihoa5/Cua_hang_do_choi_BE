using Core.Entities;

namespace Application.IService;

public interface IWarehouseRepository
{
    Task<List<Warehouse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Warehouse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Warehouse>> GetDeletedAsync(
        CancellationToken cancellationToken = default);

    Task<Warehouse?> GetDeletedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Warehouse warehouse,
        CancellationToken cancellationToken = default);

    Task RemoveDetailsAsync(
        IEnumerable<WarehouseDetail> details,
        CancellationToken cancellationToken = default);

    void Update(Warehouse warehouse);

    void Delete(Warehouse warehouse);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}