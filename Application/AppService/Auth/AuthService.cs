using Application.Exceptions;
using Application.Helper;
using Application.IService.Auth;
using Application.Model.Auth;
using Application.Model.User;
using AutoMapper;
using Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Application.AppService.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            IConfiguration configuration,
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<SignInResponse> SignInAsync(SignInRequest dto)
        {
            var loginId = dto.Username.Trim().ToLower();

            // 1. Tim user theo Email hoac Username
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == loginId ||
                    u.UserName.ToLower() == loginId);

            if (user == null)
                throw new BadRequestException("login_incorrect");

            // 2. Xac thuc mat khau
            var signInResult = await _signInManager
                .PasswordSignInAsync(
                    user,
                    dto.Password,
                    false,
                    false);

            if (!signInResult.Succeeded)
                throw new BadRequestException("login_incorrect");

            // 3. Lay roles
            var roles = await _userManager.GetRolesAsync(user);

            // 4. Sinh Access Token
            var token = JwtHelper.GenerateJwtToken(
                user,
                roles,
                _configuration,
                out var expiresOn);

            if (string.IsNullOrEmpty(token))
                throw new BadRequestException("generate_token_failed");

            var jwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            var expiryIn =
                (long)(expiresOn - DateTime.UtcNow).TotalSeconds;

            // 5. Map User
            var userDto = _mapper.Map<UserLoginResponse>(user);
            userDto.Roles = roles.ToList();

            // 6. Tra response
            return new SignInResponse
            {
                AccessToken = token,
                RefreshToken = "",
                TokenType = "Bearer",
                ExpiryIn = expiryIn,
                Expires = expiresOn,
                User = userDto
            };
        }
    }
}