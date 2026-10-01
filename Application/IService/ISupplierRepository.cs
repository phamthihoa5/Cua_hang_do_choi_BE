
using Core.Entities;

namespace Application.IService;

public interface ISupplierRepository
{
    Task<List<Supplier>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Supplier?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<Supplier>> GetDeletedAsync(
        CancellationToken cancellationToken = default);

    Task<Supplier?> GetDeletedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Supplier supplier,
        CancellationToken cancellationToken = default);

    void Update(Supplier supplier);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
