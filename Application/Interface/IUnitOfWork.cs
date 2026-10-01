using DataAccess.Repo;

namespace Application.Interface
{
    public interface IUnitOfWork
    {
        UserRepo userRepo { get; }

        // Tin tức
        NewRepo newRepo { get; }

        // Khuyến mãi
        PromotionRepo promotionRepo { get; }

        // Khuyến mãi cần truy cập sản phẩm
        ProductRepo productRepo { get; }

        Task<int> CompleteAsync();
    }
}