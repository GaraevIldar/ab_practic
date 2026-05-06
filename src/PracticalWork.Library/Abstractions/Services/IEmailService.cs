using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

/// <summary>
/// Сервис для отправки email сообщений
/// </summary>
public interface IEmailService
{
    Task<EmailSendResult> SendAsync(EmailMessage message);
}
