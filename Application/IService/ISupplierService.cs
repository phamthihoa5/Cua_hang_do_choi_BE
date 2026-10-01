
using Application.Model.Supplier;

namespace Application.IService;

public interface ISupplierService
{
    Task<List<SupplierResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SupplierResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<List<SupplierResponse>> GetDeletedAsync(
        CancellationToken cancellationToken = default);

    Task<SupplierResponse> CreateAsync(
        CreateSupplierRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    Task<SupplierResponse?> UpdateAsync(
        Guid id,
        UpdateSupplierRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default);

    Task<bool> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
