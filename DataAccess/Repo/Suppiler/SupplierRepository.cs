
using Application.IService;
using Core.Entities;
using DataAccess.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repo.Supplier;

public class SupplierRepository : ISupplierRepository
{
    private readonly AppDbContext _context;

    public SupplierRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Core.Entities.Supplier>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Core.Entities.Supplier?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Core.Entities.Supplier>> GetDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Core.Entities.Supplier?> GetDeletedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(
                x => x.Id == id && x.IsDeleted,
                cancellationToken);
    }

    public async Task AddAsync(
        Core.Entities.Supplier supplier,
        CancellationToken cancellationToken = default)
    {
        await _context.Suppliers.AddAsync(
            supplier,
            cancellationToken);
    }

    public void Update(Core.Entities.Supplier supplier)
    {
        _context.Suppliers.Update(supplier);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
