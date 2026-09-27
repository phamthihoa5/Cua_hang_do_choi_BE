using Application.Model.Ai;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface IAiService
    {
        Task<AiConsultResponseDto> ConsultToysAsync(AiConsultRequestDto req);
        Task<List<AiInventoryForecastDto>> GetInventoryForecastAsync();
        Task<AiGenerateDescResponseDto> GenerateProductDescriptionAsync(AiGenerateDescRequestDto req);
    }
}
