using Application.Core.IServices.Mail;
using Application.DTOs.Mail;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Applicaiton.Infrastructure.Services.Mail;
public class SmtpMailService:IMailService
{
    private readonly MailSettings _settings;
    private readonly ILogger<SmtpMailService> _logger;
    public SmtpMailService(IOptions<MailSettings> settings, ILogger<SmtpMailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }
    public async Task SendAsync(MailRequest request)
    {
        try
        {
            var email = new MimeMessage
            {
                Sender = new MailboxAddress(_settings.DisplayName, request.From ?? _settings.From),
                Subject = request.Subject,
                Body = new BodyBuilder
                {
                    HtmlBody = request.Body
                }.ToMessageBody()
            };
            email.To.Add(MailboxAddress.Parse(request.To));
            //using 资源 使用完即时释放资源
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex.Message, ex);
        }
    }
}
