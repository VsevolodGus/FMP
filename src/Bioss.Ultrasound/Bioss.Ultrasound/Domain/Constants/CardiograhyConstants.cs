namespace Bioss.Ultrasound.Domain.Constants
{
    internal struct CardiograhyConstants
    {

        /// <summary>
        /// Минимальное время записи для соблюдения критериев
        /// </summary>
        public const int MinRecordingDuration = 10;
        /// <summary>
        /// Максимально допустимая потеря сигнала
        /// </summary>
        public const int MaxSignalLossPercentage = 20;
        /// <summary>
        /// Базальная ЧСС, уд/мин 
        /// Минимальное значение по критерия Доуса-Редмана
        /// </summary>
        public const int MinBasalHeartRate = 110;
        /// <summary>
        /// Базальная ЧСС, уд/мин 
        /// Максмальная значение по критерия Доуса-Редмана
        /// </summary>
        public const int MaxBasalHeartRate = 160;
        /// <summary>
        /// 
        /// </summary>
        public const int MinValueSTV = 4;

        /// <summary>
        /// 
        /// </summary>
        public const int MinMovementFrequency = 3;

        public const int AbsenseSynRhythm = 2;
        public const int MaxCountDec = 2;
    }
}
