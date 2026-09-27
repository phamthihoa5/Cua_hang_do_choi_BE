namespace Application.Helper;

public class Base64Encode
{
    public static string Encode(string plainText)
    {
        // var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        // return Convert.ToBase64String(plainTextBytes).Replace("/", "_");
        
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes).TrimEnd('=');
    }
}