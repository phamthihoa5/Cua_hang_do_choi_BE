using Microsoft.AspNetCore.Mvc;
using Shared.Extensions;

namespace API.Controllers.Common;

/// <summary>
/// Api Controller Base
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ApiController : ControllerBase
{
    /// <summary>
    /// Get Locale
    /// </summary>
    /// <returns></returns>
    //protected string GetLocale()
    //{
    //    return Request.GetAcceptLanguage();
    //}
}