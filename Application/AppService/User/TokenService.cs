using Application.Exceptions;
using Application.Interface;
using Application.IService.User;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.AppService.User
{
    public class TokenService : ITokenService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ILogger<TokenService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<string> GetUserIdFromTokenAsync()
        {
            // Lấy token từ Authorization header trong HttpContext
            var authorizationHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                throw new BadRequestException("Authorization token is missing.");
            }

            // Lấy token và giải mã nó
            var token = authorizationHeader.Replace("Bearer ", "").Trim();
            string userIdStr = string.Empty;

            try
            {
                // Giải mã token JWT để lấy thông tin userId từ Claims
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Lấy userId từ claim
                userIdStr = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "id")?.Value;

                if (string.IsNullOrEmpty(userIdStr))
                {
                    throw new BadRequestException("invalid_user");
                }
            }
            catch (Exception)
            {
                throw new BadRequestException("Invalid or malformed token.");
            }

            // Kiểm tra userId trong database
            var userId = Guid.Parse(userIdStr);  // Chuyển đổi sang Guid

            // Sử dụng FirstOrDefaultAsync để lấy người dùng từ cơ sở dữ liệu
            var user = await _unitOfWork.userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.Id == userId);   // Truy vấn bất đồng bộ để lấy user

            if (user == null)
            {
                throw new BadRequestException("invalid_user");
            }

            return userIdStr;
        }
    }
}