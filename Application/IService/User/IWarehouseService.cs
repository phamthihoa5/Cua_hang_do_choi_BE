using Application.Common.Models;
using Application.Model.API;
using Application.Model.Warehouse;
using Application.Model.WarehouseDetail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IWarehouseService
    {
        Task<WarehouseResponseDto> CreateWarehouseAsync(WarehouseRequestDto request);
        Task<WarehouseResponseDto> UpdateWarehouseAsync(Guid id, WarehouseRequestDto request);
        Task<string> DeleteWarehouseAsync(Guid id, bool force = false);
        Task<PageList<WarehouseResponseDto>> GetAllAsync(QueryParam query = null);
        Task<PageList<WarehouseResponseDto>> GetAllDeleteAsync(QueryParam query = null);
        Task<WarehouseResponseDto> GetByIdAsync(Guid id);
        Task RestoreWarehouseAsync(Guid id);
    }
}
