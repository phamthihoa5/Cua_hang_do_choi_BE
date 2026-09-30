using Application.IService;
using Core.Entities;
using DataAccess.Common;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repo.Warehouse;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly AppDbContext _context;

    public WarehouseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Core.Entities.Warehouse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Include(x => x.Details)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.ImportDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Core.Entities.Warehouse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .Include(x => x.Details)
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                cancellationToken);
    }

    public async Task<List<Core.Entities.Warehouse>> GetDeletedAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Include(x => x.Details)
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.DeletedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Core.Entities.Warehouse?> GetDeletedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Warehouses
            .Include(x => x.Details)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.IsDeleted,
                cancellationToken);
    }

    public async Task AddAsync(
        Core.Entities.Warehouse warehouse,
        CancellationToken cancellationToken = default)
    {
        await _context.Warehouses.AddAsync(
            warehouse,
            cancellationToken);
    }

    public async Task RemoveDetailsAsync(
        IEnumerable<WarehouseDetail> details,
        CancellationToken cancellationToken = default)
    {
        var detailList = details.ToList();

        if (detailList.Count > 0)
        {
            _context.WarehouseDetails.RemoveRange(detailList);
        }

        await Task.CompletedTask;
    }

    public void Update(Core.Entities.Warehouse warehouse)
    {
        _context.Warehouses.Update(warehouse);
    }

    public void Delete(Core.Entities.Warehouse warehouse)
    {
        warehouse.IsDeleted = true;
        _context.Warehouses.Update(warehouse);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}