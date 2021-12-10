
public interface IAccountService:ITransientService
{
    /// <summary>
    /// 注册账号
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<IResult> RegisterAsync(RegisterRequest request,string orgin);
    /// <summary>
    /// 确认邮箱
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<IResult<string>> ConfirmEmailAsync(Guid userId, string code);
    /// <summary>
    /// 忘记密码
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request,string orgin);
    /// <summary>
    /// 重置密码
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IResult> ResetPasswordAsync(ResetPasswordRequest request);
}

