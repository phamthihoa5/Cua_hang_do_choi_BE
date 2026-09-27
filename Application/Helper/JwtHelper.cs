using Core.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Helper
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(
            ApplicationUser user,
            IEnumerable<string> roles,
            IConfiguration configuration,
            out DateTime expiresOn)
        {
            var secretKey = configuration["JwtConfiguration:Key"];
            var issuer = configuration["JwtConfiguration:Issuer"];
            var audience = configuration["JwtConfiguration:Audience"];
            var expiryMinutes = int.TryParse(configuration["JwtConfiguration:ExpiryMinutes"], out var minutes)
                ? minutes : 60;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            expiresOn = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimKey.Id, user.Id.ToString()),
                new Claim(ClaimKey.Username, user.UserName ?? ""),
                new Claim(ClaimKey.Email, user.Email ?? ""),
                new Claim(ClaimKey.Name, user.FullName ?? "")
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresOn,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
