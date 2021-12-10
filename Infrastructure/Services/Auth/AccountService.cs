
using Application.Core.IServices.Mail;
using Application.DTOs.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using static AuthorizationRoles;
public class AccountService : IAccountService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly MailSettings _mailSettings;
    private readonly IMailService _mailService;

    public AccountService(UserManager<AppUser> userManager,
        IOptions<MailSettings> mailSettings,
        IMailService mailService)
    {
        _userManager = userManager;
        _mailSettings = mailSettings.Value;
        _mailService = mailService;
    }
    public async Task<IResult> RegisterAsync(RegisterRequest model,string origin)
    {
        var userWithSameEmail = await _userManager.FindByEmailAsync(model.Email);
        if (userWithSameEmail != null)
        {
            return await Result<string>.FailAsync($"Email {model.Email} is already taken(电子邮件{model.Email}已被占用).");
        }
        var user = new AppUser
        {
            UserName = model.Username,
            Email = model.Email
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        var messages = new List<string>();
        if (result.Succeeded)
        {
            messages.Add($"User { user.UserName } Registered.(账号注册成功！)");
            //添加默认权限
            var role = await _userManager.AddToRoleAsync(user, Roles.User.ToString());
            //判断是否添加成功
            _ = role.Succeeded != true ? new Action(() => messages.Add($"User {user.UserName} Role Registered Fail(用户: {user.UserName}默认权限添加失败).")) : 
                () => messages.Add($"User {user.UserName} Registered Success(用户: {user.UserName} 注册成功).");
            //是否启用发送邮件验证账号
            if (_mailSettings.EnableVerification)
            {
                string emailVerificationUri = await GetEmailVericationUriAsync(user, origin);
                var mailRequest = new MailRequest
                {
                    From = _mailSettings.From,
                    To = user.Email,
                    Body=emailVerificationUri,
                    Subject="Confirm Registration"
                };
                await _mailService.SendAsync(mailRequest);
            }
            return await Result<string>.SuccessAsync(messages);
        }
        return await Result<string>.FailAsync(result.Errors.Select(a=>a.Description.ToString()).ToList());
    }
    public async Task<IResult<string>> ConfirmEmailAsync(Guid userId, string code)
    {
        //筛选出未验证的账号
        var user = await _userManager.Users.IgnoreQueryFilters().Where(a => a.Id == userId && !a.EmailConfirmed).FirstOrDefaultAsync();
        if (user==null)
        {
            return await Result<string>.FailAsync("An error occurred while confirming E-mail.");
        }
        code=Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
        {
            return await Result<string>.FailAsync("An error occurred while confirming {user.Email}");
        }
        return await Result<string>.SuccessAsync("Account Confirmed for E-mail.");
    }
    public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request,string origin) 
    {
        var user=await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !(await _userManager.IsEmailConfirmedAsync(user))) 
        {
            //未通过验证的邮箱无法使用
            return await Result.FailAsync("An Error has occurred!");
        }
        //获取验证token
        string code=await _userManager.GeneratePasswordResetTokenAsync(user);
        const string route = "account/reset-password";
        var endponintUri = new Uri(string.Concat($"{origin}/", route));
        //string passwordResetUrl = QueryHelpers.AddQueryString(endponintUri.ToString(),"Token",code);
        var mailRequest = new MailRequest
        {
            Body = $"Link:{endponintUri}",
            Subject = "Reset Password",
            To= user.Email
        };
        await _mailService.SendAsync(mailRequest);
        return await Result.SuccessAsync($"Password Reset Mail has been sent your authorized Email.(密码重置邮件已发送至您的授权邮箱)");
    }
    public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // 不要显示用户不存在
            return await Result<string>.ReturnErrorAsync("An Error has occurred(发生了一个错误)!");
        }
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
        if (!result.Succeeded)
        {
            return await Result<string>.ReturnErrorAsync(result.Errors.Select(a => a.Description.ToString()).ToList());
        }
        return await Result.SuccessAsync("Password Reset Successful!");
    }
    private async Task<string> GetEmailVericationUriAsync(AppUser user,string origin)
    {
        //生成账号验证码 Token
        string code=await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //将code转换url编码
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        //RUL定位
        const string route = "api/account/confirm-email";
        //起始地址
        var endpointUri=new Uri(string.Concat($"{origin}/",route));
        //组合Url
        string verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(),"userId",user.Id.ToString());
        verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
        return verificationUri;
    }

}

