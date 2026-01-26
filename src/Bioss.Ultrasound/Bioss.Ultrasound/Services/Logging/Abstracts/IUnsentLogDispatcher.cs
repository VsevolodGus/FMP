using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Logging.Abstracts
{
    public interface IUnsentLogDispatcher
    {
        /// <summary>
        /// Отправляем логи
        /// </summary>
        /// <param name="sendLogCurrentSession">логи отпрвляются из текущей сессии</param>
        /// <returns></returns>
        Task SendAllUnsentAsync(bool sendLogCurrentSession = true);
    }
}
