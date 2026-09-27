using Application.Model.Auth;
using Application.Model.User;

namespace Application.IService.Auth;

public interface IAuthService
{
    Task<SignInResponse> SignInAsync(SignInRequest dto);
    Task<SignUpResponse> SignUpAsync(SignUpRequest dto);
    Task<SignUpResponse> ForgotPassword(Guid userId, ForgotPasswordRequest request);
    Task<SignUpResponse> UpdateProfileAsync(Guid userId, UpdateInfoRequest dto);
    Task<UserProfileDto> GetProfile();
    Task<SignUpResponse> ChangePasswordAsync(ChangePasswordRequestDto dto);
    //Task<SignUpResponse> CreateStaffAccountAsync(SignUpRequest dto);
}