using PracticalWork.Library.Models;

namespace PracticalWork.Library.Abstractions.Services;

/// <summary>
/// Сервис для отправки email сообщений
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Отправка email-сообщения
    /// </summary>
    Task<EmailSendResult> SendAsync(EmailMessage message);
}
