namespace Shared.Services.ClaimService;

public interface IClaimService
{
    string GetClaim(string key);
    string GetUserId();
    string GetUserName();
    string GetRole();
    string GetName();
    string GetEmail();
}