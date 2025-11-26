namespace Bioss.Ultrasound.Services.Logging
{
    //    {LogLevel.Information, 0},
    //    {LogLevel.Warning, 1}, //1-4
    //    {LogLevel.Error, 5}, // 5-8 
    //    {LogLevel.Critical, 9},
    public enum ServerLogLevel : byte
    {
        Info = 0,
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
