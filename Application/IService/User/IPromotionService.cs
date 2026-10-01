using Application.Common.Models;
using Application.Model.API;
using Application.Model.Product;
using Application.Model.Promotion;

namespace Application.IService.User
{
    public interface IPromotionService
    {
        Task<PromotionResponseDto> CreateAsync(PromotionRequestDto dto);
        Task<PromotionResponseDto> UpdateAsync(Guid id, PromotionRequestDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<PromotionResponseDto?> GetByIdAsync(Guid id);
        Task<PromotionResponseDto?> GetBySlugAsync(string slug);
        Task<PageList<PromotionResponseDto>> GetAllAsync(QueryParam query = null);
        Task<bool> ApproveAsync(Guid id);
        Task<PageList<PromotionResponseDto>> GetIsDeletedAsync(QueryParam query = null);
        Task<string> RestorePromotionAsync(Guid id);
    }
}
