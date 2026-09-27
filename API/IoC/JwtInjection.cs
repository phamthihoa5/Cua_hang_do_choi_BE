using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API.IoC
{
    public static class JwtInjection
    {
        public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var secretKey = configuration["JwtConfiguration:Key"];
            var issuer = configuration["JwtConfiguration:Issuer"];
            var audience = configuration["JwtConfiguration:Audience"];

            if (string.IsNullOrWhiteSpace(secretKey))
                throw new InvalidOperationException("JWT Key missing from configuration");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = key,

                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,

                        NameClaimType = "name",    // Giữ nguyên claim "name" từ JWT
                        RoleClaimType = "role"     // Giữ nguyên claim "role"
                    };

                    // Debug log
                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            Console.WriteLine($"[JWT ERROR] {ctx.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = ctx =>
                        {
                            var claims = ctx.Principal?.Claims
                                .Select(c => $"{c.Type}={c.Value}");
                            Console.WriteLine("[JWT VALIDATED] " + string.Join(", ", claims ?? []));
                            return Task.CompletedTask;
                        },
                        OnMessageReceived = ctx =>
                        {
                            if (!string.IsNullOrEmpty(ctx.Token))
                                Console.WriteLine($"[JWT RECEIVED] {ctx.Token[..Math.Min(40, ctx.Token.Length)]}...");
                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
