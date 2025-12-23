using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Sessions
{
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
        Task StartSessionAsync();
        
        /// <summary>
        /// Закрывает сессию, или же закрывает текущую, если token = null
        /// </summary>
        /// <param name="token">токен закрытой сессии</param>
        /// <returns></returns>
        ValueTask Exit(string token = null);
    }
}
