using Application.Interface;
using DataAccess;
using DataAccess.Repo;
using Microsoft.EntityFrameworkCore.Storage;

namespace Application.AppService
{
    /// <summary>
    /// UnitOfWork: gom tất cả repository và quản lý transaction
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatabaseContext _context;
        private IDbContextTransaction? _transaction;

        private UserRepo? _userRepo;
        private CategoryRepo? _categoryRepo;
        private ProductRepo? _productRepo;
        private OrderRepo? _orderRepo;
        private OrderDetailsRepo? _orderDetailsRepo;
        private CartRepo? _cartRepo;
        private SupplierRepo? _supplierRepo;
        private WarehouseRepo? _warehouseRepo;
        private NewRepo? _newRepo;
        private PromotionRepo? _promotionRepo;
        private WarehouseDetailRepo? _warehouseDetailRepo;
        private PermissionRepo? _permissionRepo;
        private UserPermissionRepo? _userPermissionRepo;
        private StaffTypePermissionRepo? _staffTypePermissionRepo;

        public UnitOfWork(DatabaseContext context)
        {
            _context = context;
        }

        // Repositories (lazy init)
        public UserRepo userRepo => _userRepo ??= new UserRepo(_context);
        public CategoryRepo categoryRepo => _categoryRepo ??= new CategoryRepo(_context);
        public ProductRepo productRepo => _productRepo ??= new ProductRepo(_context);
        public OrderRepo orderRepo => _orderRepo ??= new OrderRepo(_context);
        public OrderDetailsRepo orderDetailsRepo => _orderDetailsRepo ??= new OrderDetailsRepo(_context);
        public CartRepo cartRepo => _cartRepo ??= new CartRepo(_context);
        public SupplierRepo supplierRepo => _supplierRepo ??= new SupplierRepo(_context);
        public WarehouseRepo warehouseRepo => _warehouseRepo ??= new WarehouseRepo(_context);
        public NewRepo newRepo => _newRepo ??= new NewRepo(_context);
        public PromotionRepo promotionRepo => _promotionRepo ??= new PromotionRepo(_context);
        public WarehouseDetailRepo warehouseDetailRepo => _warehouseDetailRepo ??= new WarehouseDetailRepo(_context);
        public PermissionRepo permissionRepo => _permissionRepo ??= new PermissionRepo(_context);
        public UserPermissionRepo permissionuserRepo => _userPermissionRepo ??= new UserPermissionRepo(_context);
        public StaffTypePermissionRepo staffTypePermissionRepo => _staffTypePermissionRepo ??= new StaffTypePermissionRepo(_context);

        // Save changes
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Transaction
        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started.");

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction == null) return;

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null) return;

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            await _context.DisposeAsync();
        }
    }
}
