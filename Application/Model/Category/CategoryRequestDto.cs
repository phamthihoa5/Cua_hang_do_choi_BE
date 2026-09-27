using Microsoft.AspNetCore.Http;

namespace Application.Model.Category
{
    /// <summary>
    /// Yêu cầu tạo hoặc cập nhật danh mục
    /// </summary>
    public class CategoryRequest
    {
        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// ID danh mục cha (nếu có, để phân cấp)
        /// </summary>
        public Guid? ParentId { get; set; }
        public IFormFile?  Image { get; set; }

    }
}
