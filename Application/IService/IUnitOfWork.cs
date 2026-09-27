using DataAccess.Repo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IUnitOfWork
    {
        UserRepo userRepo { get; }
        CategoryRepo categoryRepo { get; }
        ProductRepo productRepo { get; }
        OrderRepo orderRepo { get; }
        OrderDetailsRepo orderDetailsRepo { get; }
        CartRepo cartRepo { get; }
        SupplierRepo supplierRepo { get; }
        WarehouseRepo warehouseRepo { get; }
        NewRepo newRepo { get; }
        PromotionRepo promotionRepo { get; }
        WarehouseDetailRepo warehouseDetailRepo { get; }
        PermissionRepo permissionRepo { get; }
        UserPermissionRepo permissionuserRepo { get; }
        StaffTypePermissionRepo staffTypePermissionRepo { get; }

        Task<int> CompleteAsync();

        // Transaction control
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
