using Shared.Message;
using Microsoft.AspNetCore.Http;

namespace Shared.Extensions;

public static class HttpRequestExtension
{

    public static string GetAcceptLanguage(this HttpRequest request)
    {
        var locale = request.Headers["Accept-Language"].ToString();
        if (string.IsNullOrEmpty(locale))
        {
            return AppMessage.LOCALES.First().Key;
        }
        var arr = locale.Split(",");
        if (AppMessage.LOCALES.ContainsKey(arr[0]))
        {
            return arr[0];
        }
        return AppMessage.LOCALES.First().Key;
    }
}