using Application.Interface;
using Application.IService.User;
using Application.Model.Ai;
using Application.Model.Product;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class AiService : IAiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly string? _geminiApiKey;

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AiService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration config, HttpClient? httpClient = null)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _httpClient = httpClient ?? new HttpClient();
            _geminiApiKey = config["Gemini:ApiKey"] ?? config["GeminiApiKey"];
        }

        #region 1. AI Trợ lý Tư vấn Mua Đồ chơi
        public async Task<AiConsultResponseDto> ConsultToysAsync(AiConsultRequestDto req)
        {
            var allProducts = await _unitOfWork.productRepo.GetAll()
                .Where(p => !p.IsDeleted && p.ProductStatus == Core.Entities.Enum.TrangThaiSanPham.ConHang)
                .ToListAsync();

            var candidateProducts = allProducts.AsEnumerable();

            if (req.Budget.HasValue && req.Budget.Value > 0)
            {
                candidateProducts = candidateProducts.Where(p => p.Price <= req.Budget.Value);
            }

            var queryLower = req.UserMessage?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(queryLower))
            {
                candidateProducts = candidateProducts.Where(p =>
                    p.ProductName.ToLower().Contains(queryLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(queryLower))
                );
            }

            var selectedList = candidateProducts.Take(4).ToList();
            if (!selectedList.Any())
            {
                selectedList = allProducts.OrderBy(_ => Guid.NewGuid()).Take(3).ToList();
            }

            var mappedProducts = _mapper.Map<List<ProductDto>>(selectedList);

            // Thử gọi Gemini API nếu có Key
            if (!string.IsNullOrEmpty(_geminiApiKey))
            {
                try
                {
                    var productCatalogText = string.Join("\n", selectedList.Select(p => $"- {p.ProductName}: {p.Price:N0} VNĐ. Mô tả: {p.Description}"));
                    var prompt = $"Bạn là trợ lý AI chuyên nghiệp tư vấn mua đồ chơi trẻ em tại cửa hàng ToysWorld.\n" +
                                 $"Khách hàng yêu cầu: \"{req.UserMessage}\" (Tuổi bé: {req.Age}, Giới tính: {req.Gender}, Ngân sách: {req.Budget:N0} VNĐ).\n" +
                                 $"Danh sách sản phẩm phù hợp hiện có:\n{productCatalogText}\n\n" +
                                 $"Hãy đưa ra lời tư vấn ngắn gọn (khoảng 3-4 câu), thân thiện, tâm lý cho phụ huynh bằng tiếng Việt.";

                    var aiAdvice = await CallGeminiApiAsync(prompt);
                    if (!string.IsNullOrEmpty(aiAdvice))
                    {
                        return new AiConsultResponseDto
                        {
                            AdviceText = aiAdvice,
                            RecommendedProducts = mappedProducts
                        };
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Gemini API call failed: {ex.Message}. Falling back to Smart Rule AI.");
                }
            }

            // Fallback AI
            var adviceStringBuilder = new StringBuilder();
            adviceStringBuilder.Append($"Chào bạn! Dựa trên yêu cầu tư vấn");
            if (req.Age.HasValue) adviceStringBuilder.Append($" cho bé {req.Age} tuổi");
            if (req.Budget.HasValue) adviceStringBuilder.Append($" với ngân sách dưới {req.Budget:N0} đ");
            adviceStringBuilder.Append($", ToysWorld xin đề xuất các món đồ chơi an toàn, phát triển trí tuệ và tư duy tốt nhất dưới đây. Bạn có thể xem chi tiết từng món đồ chơi nhé!");

            return new AiConsultResponseDto
            {
                AdviceText = adviceStringBuilder.ToString(),
                RecommendedProducts = mappedProducts
            };
        }
        #endregion

        #region 2. AI Dự đoán Tồn kho & Cảnh báo Nhập hàng
        public async Task<List<AiInventoryForecastDto>> GetInventoryForecastAsync()
        {
            var products = await _unitOfWork.productRepo.GetAll().Where(p => !p.IsDeleted).ToListAsync();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var recentOrderDetails = await _unitOfWork.orderDetailsRepo.GetAll()
                .Include(d => d.Order)
                .Where(d => !d.IsDeleted && d.Order.OrderDate >= thirtyDaysAgo)
                .ToListAsync();

            var forecastList = new List<AiInventoryForecastDto>();

            foreach (var prod in products)
            {
                var sold30Days = recentOrderDetails.Where(d => d.IdProduct == prod.Id).Sum(d => d.Quantity);
                var dailyRate = sold30Days / 30.0;
                var currentStock = prod.Quantity;

                int daysRemaining = 999;
                if (dailyRate > 0)
                {
                    daysRemaining = (int)Math.Floor(currentStock / dailyRate);
                }

                string urgency = "Normal";
                int suggestedRestock = 0;
                string aiAnalysis = "";

                if (currentStock <= 5 || daysRemaining <= 7)
                {
                    urgency = "Critical";
                    suggestedRestock = Math.Max(30, (int)(dailyRate * 30) - currentStock);
                    aiAnalysis = $"⚠️ Tồn kho báo động đỏ! Tốc độ bán {dailyRate:F1} sp/ngày. Dự kiến hết hàng trong {daysRemaining} ngày. Cần nhập gấp {suggestedRestock} sp.";
                }
                else if (daysRemaining <= 15)
                {
                    urgency = "Warning";
                    suggestedRestock = Math.Max(20, (int)(dailyRate * 20) - currentStock);
                    aiAnalysis = $"⚡ Tồn kho ở mức cảnh báo. Dự kiến hết hàng trong {daysRemaining} ngày. Khuyên nhập thêm {suggestedRestock} sp.";
                }
                else
                {
                    urgency = "Normal";
                    suggestedRestock = 0;
                    aiAnalysis = $"✅ Tồn kho ổn định. Đủ bán trong {Math.Min(daysRemaining, 90)} ngày tới.";
                }

                forecastList.Add(new AiInventoryForecastDto
                {
                    ProductId = prod.Id,
                    ProductName = prod.ProductName,
                    CurrentStock = currentStock,
                    Sold30Days = sold30Days,
                    DailySalesRate = Math.Round(dailyRate, 2),
                    DaysRemaining = daysRemaining,
                    SuggestedRestockQty = suggestedRestock,
                    Urgency = urgency,
                    AiAnalysisText = aiAnalysis
                });
            }

            return forecastList.OrderByDescending(f => f.Urgency == "Critical" ? 3 : f.Urgency == "Warning" ? 2 : 1).ToList();
        }
        #endregion

        #region 3. AI Tự động Tạo Mô tả Sản phẩm & SEO
        public async Task<AiGenerateDescResponseDto> GenerateProductDescriptionAsync(AiGenerateDescRequestDto req)
        {
            if (string.IsNullOrEmpty(req.ProductName))
            {
                return new AiGenerateDescResponseDto
                {
                    Description = "Vui lòng nhập tên sản phẩm.",
                    Highlights = "Chưa có thông tin",
                    SeoDescription = "Chưa có thông tin"
                };
            }

            if (!string.IsNullOrEmpty(_geminiApiKey))
            {
                try
                {
                    var prompt = $"Bạn là chuyên gia marketing bài viết cho Cửa hàng đồ chơi trẻ em ToysWorld.\n" +
                                 $"Tạo mô tả sản phẩm cho đồ chơi: \"{req.ProductName}\", Danh mục: \"{req.CategoryName ?? "Đồ chơi phát triển tư duy"}\", Nhóm tuổi: \"{req.AgeGroup ?? "3+"}\", Từ khóa: \"{req.Keywords}\".\n\n" +
                                 $"Trả về định dạng JSON thuần dạng:\n" +
                                 $"{{\n" +
                                 $"  \"description\": \"Đoạn mô tả chi tiết hấp dẫn 150-200 từ...\",\n" +
                                 $"  \"highlights\": \"- Chất liệu an toàn ABS\\n- Phát triển tư duy logic\\n- Màu sắc bắt mắt\",\n" +
                                 $"  \"seoDescription\": \"Mô tả ngắn gọn chuẩn SEO 15-20 từ...\"\n" +
                                 $"}}";

                    var jsonResp = await CallGeminiApiAsync(prompt);
                    if (!string.IsNullOrEmpty(jsonResp))
                    {
                        var cleanJson = ExtractJsonFromString(jsonResp);
                        var parsed = JsonSerializer.Deserialize<AiGenerateDescResponseDto>(cleanJson, _jsonOptions);
                        if (parsed != null && !string.IsNullOrEmpty(parsed.Description))
                        {
                            return parsed;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Gemini API description generation failed: {ex.Message}. Falling back to template AI.");
                }
            }

            // Fallback Generator
            var category = !string.IsNullOrEmpty(req.CategoryName) ? req.CategoryName : "Đồ chơi phát triển trí tuệ";
            var age = !string.IsNullOrEmpty(req.AgeGroup) ? req.AgeGroup : "3 tuổi trở lên";

            return new AiGenerateDescResponseDto
            {
                Description = $"Sản phẩm '{req.ProductName}' là dòng đồ chơi cao cấp thuộc danh mục {category}, dành riêng cho trẻ từ {age}. Sản phẩm được thiết kế tinh xảo, màu sắc bắt mắt, giúp bé vừa chơi vừa phát triển khả năng sáng tạo, tư duy logic và kỹ năng vận động tinh. Chất liệu nhựa ABS cao cấp an toàn tuyệt đối cho bé.",
                Highlights = $"• Chất liệu nhựa ABS nguyên sinh cao cấp, an toàn không độc hại\n• Giúp bé phát triển tư duy logic và khả năng sáng tạo\n• Mẫu mã hiện đại, màu sắc tươi sáng hấp dẫn bé\n• Thích hợp làm quà tặng sinh nhật, Lễ Tết cho trẻ",
                SeoDescription = $"Mua ngay {req.ProductName} chính hãng cho bé từ {age} tại ToysWorld. Chất liệu an toàn, phát triển tư duy, giá tốt nhất thị trường."
            };
        }
        #endregion

        #region Helper Methods (Gemini REST API Call)
        private async Task<string?> CallGeminiApiAsync(string promptText)
        {
            if (string.IsNullOrEmpty(_geminiApiKey)) return null;

            var models = new[] { "gemini-2.5-flash", "gemini-1.5-flash-latest", "gemini-3.6-flash", "gemini-2.5-pro" };

            foreach (var model in models)
            {
                try
                {
                    var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={_geminiApiKey.Trim()}";

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = promptText }
                                }
                            }
                        }
                    };

                    var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                    var response = await _httpClient.PostAsync(url, jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var respJson = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(respJson);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                        {
                            var candidate = candidates[0];
                            if (candidate.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                            {
                                var text = parts[0].GetProperty("text").GetString();
                                if (!string.IsNullOrEmpty(text))
                                {
                                    Console.WriteLine($"✅ Gemini API ({model}) returned response successfully.");
                                    return text;
                                }
                            }
                        }
                    }
                    else
                    {
                        var errStr = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"⚠️ Gemini API ({model}) Error ({response.StatusCode}): {errStr}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Gemini API ({model}) Exception: {ex.Message}");
                }
            }

            return null;
        }

        private string ExtractJsonFromString(string input)
        {
            var start = input.IndexOf('{');
            var end = input.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                return input.Substring(start, end - start + 1);
            }
            return input;
        }
        #endregion
    }
}
