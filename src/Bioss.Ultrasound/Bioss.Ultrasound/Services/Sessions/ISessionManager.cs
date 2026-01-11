using Bioss.Ultrasound.Data.Database.Entities;
using System;
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
        

        ValueTask Exit(SessionEntity session);
        ValueTask Exit();

    }
}
