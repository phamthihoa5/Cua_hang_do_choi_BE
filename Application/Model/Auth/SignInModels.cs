using Application.Model.User;

namespace Application.Model.Auth;

public class SignInRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class SignInResponse
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? TokenType { get; set; } = "bearer";
    public long ExpiryIn { get; set; }
    public DateTime Expires { get; set; }
    public UserLoginResponse? User { get; set; }
}

public class ForgotPasswordRequest
{
    public string Password { set; get; } = "";
    public string ConfirmPassword { set; get; } = "";
}