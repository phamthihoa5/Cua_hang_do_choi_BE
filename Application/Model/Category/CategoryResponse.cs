namespace Application.Model.Category
{
    /// <summary>
    /// DTO trả về khi lấy thông tin danh mục
    /// </summary>
    public class CategoryResponse
    {
        /// <summary>
        /// ID danh mục
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// Slug (dùng cho SEO / URL thân thiện)
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// ID danh mục cha (nếu có)
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Tên danh mục cha (nếu cần hiển thị nhanh)
        /// </summary>
        public string? ParentName { get; set; }

        public string? Image { get; set; }

        /// <summary>
        /// Danh sách danh mục con
        /// </summary>
        public List<CategoryResponse> Children { get; set; }

        public bool IsDeleted { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string CreatedbyStr { get; set; } = "system";
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string CategoryName { get; set; } = "";

        public Guid? ParentId { get; set; }
        public string? ParentName { get; set; }
    }
}