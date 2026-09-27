using Application.Common.Models;
using Application.Model.API;
using Application.Model.Supplier;

namespace Application.IService.User
{
    public interface ISupplierService
    {
        Task<SupplierResponseDto> CreateSupplierAsync(SupplierRequestDto createNews);
        Task<SupplierResponseDto> UpdateSupplierAsync(Guid id, SupplierRequestDto updateNews);
        Task<string> DeleteSupplierAsync(Guid id);
        Task<PageList<SupplierResponseDto>> GetAllAsync(QueryParam query = null);
        Task<PageList<SupplierResponseDto>> GetAllAsyncDelete(QueryParam query = null);
        Task RestoreSupplierAsync(Guid id);
        Task<SupplierResponseDto> GetByIdAsync(Guid id);
    }
}
