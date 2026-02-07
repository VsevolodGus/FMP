namespace Bioss.Ultrasound.Services.Logging
{
    public enum ServerLogLevel : byte
    {
        /// <summary>
        /// Информационный лог
        /// </summary>
        Info = 0,
        /// <summary>
        /// Глобальные критические ошибки, которые создают нагрузку
        /// </summary>
        Warn= 5,

        /// <summary>
        /// Ошибки блютуза
        /// </summary>
        BluetoothError = 6,
        /// <summary>
        /// Глобальные критические ошибки, которые создают нагрузку
        /// </summary>
        ServerError = 7,
        /// <summary>
        /// Глобальные критические ошибки, которые создают нагрузку
        /// </summary>
        CriticalFunctionalityError = 8,
        /// <summary>
        /// Глобальные критические ошибки, из-за которых приложение закрывается
        /// </summary>
        FatalTerminationError = 9
    }
}
