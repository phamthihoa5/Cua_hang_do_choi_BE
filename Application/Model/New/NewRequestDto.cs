using Microsoft.AspNetCore.Http;

namespace Application.Model.News
{
    /// <summary>
    /// DTO yêu cầu tạo/cập nhật tin tức
    /// </summary>
    public class NewsRequestDto
    {
        /// <summary>
        /// Tiêu đề tin tức
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Nội dung tin tức
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Ảnh đại diện cho tin tức (nếu có)
        /// </summary>
        public List<IFormFile>? Image { get; set; }
    }
}
