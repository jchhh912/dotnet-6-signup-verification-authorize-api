
using Application.DTOs.Mail;

namespace Application.Core.IServices.Mail;
public interface IMailService : ITransientService
{
    Task SendAsync(MailRequest request);
}

