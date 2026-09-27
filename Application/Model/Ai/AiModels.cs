using Application.Model.Product;
using System.Collections.Generic;

namespace Application.Model.Ai
{
    public class AiConsultRequestDto
    {
        public string UserMessage { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string? Gender { get; set; } // "Nam", "Nữ", "Khác"
        public decimal? Budget { get; set; }
    }

    public class AiConsultResponseDto
    {
        public string AdviceText { get; set; } = string.Empty;
        public List<ProductDto> RecommendedProducts { get; set; } = new List<ProductDto>();
    }

    public class AiInventoryForecastDto
    {
        public System.Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Sold30Days { get; set; }
        public double DailySalesRate { get; set; }
        public int DaysRemaining { get; set; }
        public int SuggestedRestockQty { get; set; }
        public string Urgency { get; set; } = "Normal"; // "Critical", "Warning", "Normal"
        public string AiAnalysisText { get; set; } = string.Empty;
    }

    public class AiGenerateDescRequestDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? AgeGroup { get; set; }
        public string? Keywords { get; set; }
    }

    public class AiGenerateDescResponseDto
    {
        public string Description { get; set; } = string.Empty;
        public string Highlights { get; set; } = string.Empty;
        public string SeoDescription { get; set; } = string.Empty;
    }
}
