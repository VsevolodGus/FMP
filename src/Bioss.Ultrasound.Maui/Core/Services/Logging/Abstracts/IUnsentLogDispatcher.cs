namespace Bioss.Ultrasound.Core.Services.Logging.Abstracts;

public interface IUnsentLogDispatcher
{
    /// <summary>
    /// Отправляем логи
    /// </summary>
    /// <param name="sendLogCurrentSession">логи отпрвляются из текущей сессии</param>
    /// <returns></returns>
    Task SendAllUnsentAsync(bool sendLogCurrentSession = true);
}
