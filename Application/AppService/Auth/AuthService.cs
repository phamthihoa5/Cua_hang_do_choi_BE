using Application.Exceptions;
using Application.Helper;
using Application.Interface;
using Application.IService;
using Application.IService.Auth;
using Application.IService.User;
using Application.Model;
using Application.Model.Auth;
using Application.Model.User;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;
using DataAccess.Repo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Constants;
using Shared.Helpers;
using Shared.Logger;
using Shared.Message;
using Shared.Services.ClaimService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.AppService.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService, 
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger
            )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ===================== SIGN UP =====================
        public async Task<SignUpResponse> SignUpAsync(SignUpRequest dto)
        {
            if (dto.Password != dto.ConfirmPassword)
                throw new BadRequestException("confirm_password_invalid");

            var email = dto.Email?.Trim().ToLower().RemoveSpace() ?? throw new BadRequestException("email_required");
            var normalizedEmail = dto.Email?.Trim().ToUpper().RemoveSpace() ?? email.ToUpper();
            var phone = dto.PhoneNumber?.RemoveSpace();

            // 🔹 Kiểm tra email trùng
            var existedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existedUser != null)
                throw new BadRequestException("email_already_exists");

            try
            {
                // 🔹 Tạo user mới
                var user = new ApplicationUser
                {
                    UserName = email,
                    NormalizedUserName = normalizedEmail,
                    Email = email,
                    NormalizedEmail = normalizedEmail,
                    FullName = dto.FullName?.Trim().RemoveSpace(" "),
                    PhoneNumber = phone,
                    PhoneNumberConfirmed = !string.IsNullOrEmpty(phone),
                    EmailConfirmed = true,
                    Gender = dto.Gender,
                    Address = dto.Address,
                    CreatedBy = dto.FullName?.Trim().RemoveSpace(" "),
                    CreatedOn = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    throw new BadRequestException(string.Join(" | ", result.Errors.Select(e => e.Description)));

                // 🔹 Mặc định role là Customer
                string roleToAssign = RoleName.Customer;

                try
                {
                    // ✅ Lấy role trực tiếp từ token (TokenService)
                    var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authorizationHeader))
                    {
                        var token = authorizationHeader.Replace("Bearer ", "").Trim();
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(token);

                        // Lấy claim role (hỗ trợ cả "role" và ClaimTypes.Role)
                        var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
                            c.Type.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                            c.Type.Equals("role", StringComparison.OrdinalIgnoreCase))?.Value;

                        if (!string.IsNullOrEmpty(roleClaim))
                        {
                            // Nếu token là của Manager → user mới là Staff
                            if (roleClaim.Equals(RoleName.Manager, StringComparison.OrdinalIgnoreCase))
                            {
                                roleToAssign = RoleName.Staff;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Cannot determine roleToAssign, defaulting to Customer");
                }

                // 🔹 Gán role cho user mới
                var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
                if (!roleResult.Succeeded)
                    throw new BadRequestException(string.Join(" | ", roleResult.Errors.Select(e => e.Description)));

                // 🔹 Trả kết quả
                return new SignUpResponse
                {
                    Ok = true,
                    Message = "sign_up_success",
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SignUp error");
                throw;
            }
        }



        // ===================== SIGN IN =====================
        public async Task<SignInResponse> SignInAsync(SignInRequest dto)
        {
            var loginId = dto.Username.Trim().ToLower();

            // 1. Tìm user theo Email hoặc Username
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == loginId || u.UserName.ToLower() == loginId);

            if (user == null)
                throw new BadRequestException("login_incorrect");

            // 2. Xác thực mật khẩu
            var signInResult = await _signInManager.PasswordSignInAsync(user, dto.Password, false, false);
            if (!signInResult.Succeeded)
                throw new BadRequestException("login_incorrect");

            // 3. Lấy roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Sinh Access Token
            var token = JwtHelper.GenerateJwtToken(user, roles, _configuration, out var expiresOn);
            if (string.IsNullOrEmpty(token))
                throw new BadRequestException("generate_token_failed");

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var expiryIn = (long)(expiresOn - DateTime.UtcNow).TotalSeconds;

            // 5. Map User
            var userDto = _mapper.Map<UserLoginResponse>(user);
            userDto.Roles = roles.ToList();

            // 6. Trả response
            return new SignInResponse
            {
                AccessToken = token,
                RefreshToken = "", // Nếu có refresh token thì bổ sung
                TokenType = "Bearer", // 🔥 Quan trọng: chữ B phải hoa
                ExpiryIn = expiryIn,
                Expires = expiresOn,
                User = userDto
            };
        }



        // ===================== CHANGE PASSWORD =====================
        public async Task<SignUpResponse> ChangePasswordAsync(ChangePasswordRequestDto dto)
        {
            try
            {
                var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    throw new BadRequestException("invalid_user");
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null) throw new BadRequestException("User not found");

                var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);
                if (!result.Succeeded)
                    throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

                return new SignUpResponse { Ok = true };
            }
            catch (BadRequestException e)
            {
                throw new BadRequestException(e.Message);
            }
            catch (Exception e)
            {
                Logging.Error($"Error change password handler: {e.Message}\n{e.StackTrace}", e);
                throw new BadRequestException("change_password_error");
            }
            finally
            {
                //await _unitOfWork.DisposeAsync();
            }
        }

        // ===================== FORGOT PASSWORD =====================
        public async Task<SignUpResponse> ForgotPassword(Guid userId, ForgotPasswordRequest request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    throw new BadRequestException("user_not_found");

                // Tạo token reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                // Reset password
                var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(" | ", result.Errors.Select(e => e.Description));
                    throw new BadRequestException(errors);
                }

                // Nếu bạn muốn lưu thêm log reset password thì mới cần UoW transaction
                // await _unitOfWork.CompleteAsync();
                // await _unitOfWork.CommitAsync();

                return new SignUpResponse
                {
                    Ok = true,
                    Message = "reset_password_success",
                    UserId = user.Id
                };
            }
            catch (BadRequestException)
            {
                throw; // giữ nguyên message từ validation
            }
            catch (Exception e)
            {
                Logging.Error($"Error forgot password handler: {e.Message}\n{e.StackTrace}", e);
                throw new BadRequestException("forgot_password_error");
            }
            finally
            {
                //await _unitOfWork.DisposeAsync();
            }
        }


        // ===================== GET PROFILE =====================
        public async Task<UserProfileDto> GetProfile()
        {
            try
            {
                var userIdStr = await _tokenService.GetUserIdFromTokenAsync();
                if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                    throw new BadRequestException("invalid_user");

                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    throw new BadRequestException("user_not_found");

                var roles = await _userManager.GetRolesAsync(user);

                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    Gender = user.Gender,
                    Address = user.Address,
                    Roles = roles.ToList(),
                    CreatedOn = user.CreatedOn,
                    CreatedBy = user.CreatedBy
                };
            }
            catch (BadRequestException)
            {
                throw;
            }
            catch (Exception e)
            {
                Logging.Error($"Error get profile handler: {e.Message}\n{e.StackTrace}", e);
                throw new BadRequestException("get_profile_error");
            }
        }




        // ===================== UPDATE PROFILE =====================
        public async Task<SignUpResponse> UpdateProfileAsync(Guid userId, UpdateInfoRequest dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null) throw new BadRequestException("User not found");

                user.FullName = dto.FullName.RemoveSpace(" ");
                user.PhoneNumber = dto.PhoneNumber;
                user.UpdatedBy = dto.FullName.RemoveSpace(" ");
                user.UpdatedOn = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

                return new SignUpResponse { Ok = true };
            }
            catch (BadRequestException e)
            {
                throw new BadRequestException(e.Message);
            }
            catch (Exception e)
            {
                Logging.Error($"Error update profile handler: {e.Message}\n{e.StackTrace}", e);
                throw new BadRequestException("update_profile_error");
            }
            finally
            {
                //await _unitOfWork.DisposeAsync();
            }
        }
    }
}
