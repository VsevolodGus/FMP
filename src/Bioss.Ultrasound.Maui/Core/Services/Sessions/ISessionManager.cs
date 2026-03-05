namespace Bioss.Ultrasound.Core.Services.Sessions;

public interface ISessionManager
{
    /// <summary>
    /// Получение сессии, если сессии нет, то создает новую
    /// </summary>
    /// <returns>Данные текущей сессии</returns>
    ValueTask<SessionInfo> GetCurrentSessionAsync();

    /// <summary>
    /// Создает новую сессию
    /// </summary>
    /// <returns></returns>
    Task<SessionInfo> StartSessionAsync();

    /// <summary>
    /// Закрытие сессии
    /// </summary>
    /// <returns></returns>
    Task Exit(string token);
}
