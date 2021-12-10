using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class TokenService : ITokenService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly MailSettings _mailSettings;
    public TokenService(UserManager<AppUser> userManager, 
        IOptions<JwtSettings> jwtSettings,
        IOptions<MailSettings> mailSettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _mailSettings= mailSettings.Value;
    }
    public async Task<IResult<TokenResponse>> GetTokenAsync(TokenRequest model, string ipAddress)
    {
        //是否存在用户
        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user == null) return await Result<TokenResponse>.FailAsync("user notfound(未找到用户)");
        if (!await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return await Result<TokenResponse>.FailAsync("user notfound(未找到用户)");
        }
        if (_mailSettings.EnableVerification&&!user.EmailConfirmed)
        {
            return await Result<TokenResponse>.FailAsync("email not confirmed.");
        }
        //创建刷新令牌
        user.RefreshToken = GenerateRefreshToken();
        //刷新令牌过期时间
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        //更新模型
        await _userManager.UpdateAsync(user);
        //生成Token
        string token = await GenerateJwtAsync(user, ipAddress);
        var response = new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
        return await Result<TokenResponse>.SuccessAsync(response);
    }

    public async Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
    {
        //判断模型是否为空
        if (request is null)
        {
            throw new InvalidDataException("invalid token(无效令牌)");
        }
        //解析用户令牌
        var userPrincipal = GetPrincipalFromExpiredToken(request.Token);
        string userEmail = userPrincipal.FindFirstValue(ClaimTypes.Email);
        //查找用户
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user == null)
        {
            return await Result<TokenResponse>.FailAsync("user notfound(未找到用户)");
        }
        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return await Result<TokenResponse>.FailAsync("invalid credentials(无效凭证)");
        }
        //生成令牌
        string token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user, ipAddress));
        //更新刷新令牌
        user.RefreshToken = GenerateRefreshToken();
        //更新令牌过期时间
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);
        //更新模型
        await _userManager.UpdateAsync(user);
        var respons = new TokenResponse(token, user.RefreshToken, user.RefreshTokenExpiryTime);
        return await Result<TokenResponse>.SuccessAsync(respons);
    }

    #region Token辅助方法
    //生成Jwt令牌
    private async Task<string> GenerateJwtAsync(AppUser user, string ipAddress)
    {
        return GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user, ipAddress));
    }
    /// <summary>
    /// 获取身份信息票据
    /// </summary>
    /// <param name="user"></param>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    private async Task<IEnumerable<Claim>> GetClaimsAsync(AppUser user, string ipAddress)
    {
        //var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name,user.UserName),
                new("ipAddress", ipAddress)
            };
        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }
        return claims;

    }
    /// <summary>
    /// 生成加密令牌
    /// </summary>
    /// <param name="signingCredentials"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
    {
        //生成加密的Token
        var token = new JwtSecurityToken(
           issuer: _jwtSettings.Issuer,
           audience: _jwtSettings.Audience,
           claims: claims,
           expires: DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
           signingCredentials: signingCredentials);
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }
    /// <summary>
    /// 生成更新令牌
    /// </summary>
    /// <returns></returns>
    private static string GenerateRefreshToken()
    {
        var rng = RandomNumberGenerator.Create();
        var randomNumber = new byte[32];
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    /// <summary>
    /// 解析令牌
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(
                SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
        {
            throw new InvalidDataException("invalid token(无效令牌)");
        }

        return principal;
    }
    /// <summary>
    /// 返回加密后的签名凭证
    /// </summary>
    /// <returns></returns>
    private SigningCredentials GetSigningCredentials()
    {
        byte[] secret = Encoding.UTF8.GetBytes(_jwtSettings.Key);
        return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
    }
    #endregion
}
