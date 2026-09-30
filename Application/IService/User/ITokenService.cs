using System.Threading.Tasks;

namespace Application.IService.User
{
    public interface ITokenService
    {
        Task<string> GetUserIdFromTokenAsync();
    }
}