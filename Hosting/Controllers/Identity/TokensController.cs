using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;
    public TokensController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }
    /// <summary>
    /// 本地用户登录获取令牌
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("account/token")]
    public async Task<IActionResult> GetTokenAsync(TokenRequest model)
    {
        var result = await _tokenService.GetTokenAsync(model, GenerateIPAddress());
        return Ok(result);
    }
    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("token/refresh-token")]
    public async Task<ActionResult> RefreshAsync(RefreshTokenRequest request)
    {
        var response = await _tokenService.RefreshTokenAsync(request, GenerateIPAddress());
        return Ok(response);
    }
    private string GenerateIPAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"];
        }
        else
        {
            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
