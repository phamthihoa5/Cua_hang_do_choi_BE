using System.Text;
using System.Text.RegularExpressions;

namespace Shared.Helpers;

public static class StringHelper
{
    private static string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public static string RemoveSpace(this string text, string replaceTo = "")
    {
        return Regex.Replace(text, @"\s+", replaceTo);
    }

    public static bool HasPlaceholder(this string s)
    {
        return Regex.IsMatch(s, "{\\d+}");
    }

    public static string RandomString(int length = 32)
    {
        var random = new Random();
        return new string(Enumerable.Repeat(Chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GetUrl(string host, string uri)
    {
        try
        {
            var splitHost = host.Split("//");
            var http = splitHost[0];
            var domain = splitHost.Skip(1).ToArray();
            var newDomain = $"{string.Join("/", domain)}/{uri}";
            return $"{http}//{newDomain.Replace("//", "/")}";
        }
        catch (Exception e)
        {
            return host + uri;
        }
    }

    public static string RemoveLeadingSlash(this string input)
    {
        if (string.IsNullOrEmpty(input) || input[0] != '/') return input;

        var resultBuilder = new StringBuilder(input);
        resultBuilder.Remove(0, 1);
        return resultBuilder.ToString();
    }


    public static string ExtractPath(string originalPath)
    {
        var parts = originalPath.Split('/');
        if (parts.Length >= 2)
        {
            return $"{parts[0]}/{parts[^1]}";
        }
        return originalPath;
    }

}