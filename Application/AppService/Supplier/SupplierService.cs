
using System.Net.Mail;
using Application.IService;
using Application.Model.Supplier;
using Core.Entities;

namespace Application.AppService.Supplier;

public class SupplierService : ISupplierService
{
    private readonly ISupplierRepository _repository;

    public SupplierService(ISupplierRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SupplierResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var suppliers = await _repository.GetAllAsync(cancellationToken);
        return suppliers.Select(MapToResponse).ToList();
    }

    public async Task<SupplierResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(id, cancellationToken);
        return supplier == null ? null : MapToResponse(supplier);
    }

    public async Task<List<SupplierResponse>> GetDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        var suppliers = await _repository.GetDeletedAsync(cancellationToken);
        return suppliers.Select(MapToResponse).ToList();
    }

    public async Task<SupplierResponse> CreateAsync(
        CreateSupplierRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        Validate(request.Name, request.Phone, request.Email, request.Address);

        var supplier = new Core.Entities.Supplier
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email.Trim(),
            Address = request.Address.Trim(),
            Note = request.Note?.Trim(),
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _repository.AddAsync(supplier, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse?> UpdateAsync(
        Guid id,
        UpdateSupplierRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
            return null;

        Validate(request.Name, request.Phone, request.Email, request.Address);

        supplier.Name = request.Name.Trim();
        supplier.Phone = request.Phone.Trim();
        supplier.Email = request.Email.Trim();
        supplier.Address = request.Address.Trim();
        supplier.Note = request.Note?.Trim();
        supplier.UpdatedBy = string.IsNullOrWhiteSpace(updatedBy) ? "system" : updatedBy;
        supplier.UpdatedAt = DateTime.UtcNow;

        _repository.Update(supplier);
        await _repository.SaveChangesAsync(cancellationToken);

        return MapToResponse(supplier);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
            return false;

        supplier.IsDeleted = true;
        supplier.DeletedBy = string.IsNullOrWhiteSpace(deletedBy) ? "system" : deletedBy;
        supplier.DeletedAt = DateTime.UtcNow;

        _repository.Update(supplier);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RestoreAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var supplier = await _repository.GetDeletedByIdAsync(id, cancellationToken);
        if (supplier == null)
            return false;

        supplier.IsDeleted = false;
        supplier.DeletedBy = null;
        supplier.DeletedAt = null;

        _repository.Update(supplier);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void Validate(
        string? name,
        string? phone,
        string? email,
        string? address)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tên nhà cung cấp không được để trống.");

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Số điện thoại không được để trống.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email không được để trống.");

        if (!MailAddress.TryCreate(email.Trim(), out _))
            throw new ArgumentException("Email không đúng định dạng.");

        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Địa chỉ không được để trống.");
    }

    private static SupplierResponse MapToResponse(Core.Entities.Supplier supplier)
    {
        return new SupplierResponse
        {
            Id = supplier.Id,
            Name = supplier.Name,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            Note = supplier.Note,
            CreatedBy = supplier.CreatedBy,
            CreatedAt = supplier.CreatedAt,
            UpdatedBy = supplier.UpdatedBy,
            UpdatedAt = supplier.UpdatedAt,
            DeletedBy = supplier.DeletedBy,
            DeletedAt = supplier.DeletedAt,
            IsDeleted = supplier.IsDeleted
        };
    }
}
