using Microsoft.AspNetCore.Http;
using Shared.Constants;
using System.Security.Claims;

namespace Shared.Services.ClaimService;

public class ClaimService : IClaimService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string GetClaim(string key)
        => User?.FindFirst(key)?.Value ?? string.Empty;

    public string GetUserId()
        => User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    public string GetUserName()
        => GetClaim(ClaimKey.Username); // giữ lại nếu bạn vẫn cần username đăng nhập

    public string GetRole()
        => GetClaim(ClaimKey.Role);

    public string GetEmail()
        => GetClaim(ClaimKey.Email);

    /// <summary>
    /// Lấy tên hiển thị của người dùng (ưu tiên Name, nếu không có fallback về Username)
    /// </summary>
    public string GetName()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return "system";

        var name = user.FindFirst("name")?.Value
                 ?? user.FindFirst(ClaimTypes.Name)?.Value
                 ?? user.FindFirst(ClaimTypes.GivenName)?.Value
                 ?? user.FindFirst("preferred_username")?.Value
                 ?? user.FindFirst(ClaimKey.Username)?.Value
                 ?? "system";

        return name.Trim();
    }


}
