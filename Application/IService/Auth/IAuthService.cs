using Application.Model.Auth;

namespace Application.IService.Auth
{
    public interface IAuthService
    {
        Task<SignInResponse> SignInAsync(SignInRequest dto);
    }
}