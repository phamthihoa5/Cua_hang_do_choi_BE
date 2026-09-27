using Application.Interface;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class PromotionCleanupService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public PromotionCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Chạy công việc mỗi giờ để kiểm tra và xử lý khuyến mãi hết hạn
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _serviceProvider.CreateScope())  // Tạo scope để sử dụng các dịch vụ scoped
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Lấy tất cả các khuyến mãi chưa bị xóa và đã hết hạn
                var expiredPromotions = await unitOfWork.promotionRepo
                    .GetAll()
                    .Where(p => !p.IsDeleted && p.EndDate < DateTime.UtcNow)
                    .Include(p => p.Products)
                    .ToListAsync();

                foreach (var promotion in expiredPromotions)
                {
                    // Đánh dấu khuyến mãi là đã xóa mềm
                    promotion.IsDeleted = true;

                    // Loại bỏ khuyến mãi khỏi các sản phẩm
                    foreach (var product in promotion.Products)
                    {
                        product.Promotion = null;  // Loại bỏ khuyến mãi khỏi sản phẩm
                    }

                    // Cập nhật khuyến mãi và các sản phẩm
                    await unitOfWork.promotionRepo.Update(promotion);

                    // Cập nhật lại các sản phẩm có khuyến mãi đã hết hạn
                    foreach (var product in promotion.Products)
                    {
                        await unitOfWork.productRepo.Update(product);
                    }
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                await unitOfWork.CompleteAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
