public interface ITokenService : ITransientService
{

    /// <summary>
    /// 获取Token(登陆验证)
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<IResult<TokenResponse>> GetTokenAsync(TokenRequest model, string ipAddress);
    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    Task<IResult<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
}