using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Model.Auth
{
    public class JwtSettings
    {
        public string TokenType { get; set; } = "Bearer";
        public string Key { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Issuer { get; set; } = "";
        public int TokenValidityInMinutes { get; set; } = 500;
        public int RefreshTokenValidityInDays { get; set; } = 7;
    }
}
