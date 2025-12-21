namespace Bioss.Ultrasound.Domain.Models
{
    public class CardiotocographyInfo
    {
        #region Основные параметры
        /// <summary>
        /// Длительность записи, мин
        /// </summary>
        public float? RecordingDuration { get; set; }
        /// <summary>
        /// Соответствует критерию
        /// </summary>
        public bool IsValidRecordingDuration { get; set; }
        /// <summary>
        /// Потери сигнала, %
        /// </summary>
        public float? SignalLossPercentage { get; set; }
        /// <summary>
        /// Соответствует критерию
        /// </summary>
        public bool SignalLossValid { get; set; }

        /// <summary>
        /// Базальная ЧСС, уд/мин
        /// </summary>
        public float? BasalHeartRate { get; set; }
        /// <summary>
        /// Соответствует критерию
        /// </summary>
        public bool BasalHeartRateValid { get; set; } 
        #endregion

        #region Акцелерации
        /// <summary>
        ///  Кол-во Акц. > 10 уд/мин
        /// </summary>
        public int? AccelerationsOver10 { get; set; }
        /// <summary>
        /// Кол-во Акц. > 15 уд/мин
        /// </summary>
        public int? AccelerationsOver15 { get; set; }
        #endregion

        #region Децелерации
        /// <summary>
        /// Кол-во Дец. > 20 уд./мин
        /// </summary>
        public int? Decelerations { get; set; }
        /// <summary>
        /// Валидность децелераций
        /// </summary>
        public bool DecelerationsMark { get; set; }
        #endregion

        #region Вариабельность
        /// <summary>
        /// Высокая вариаб., мин
        /// </summary>
        public int? HighVariabilityMinutes { get; set; }
        /// <summary>
        /// Низкая вариаб., мин
        /// </summary>
        public int? LowVariabilityMinutes { get; set; }
        #endregion

        #region Синхронный ритм
        /// <summary>
        /// Син. ритм, мин 
        /// </summary>
        public int? SyncRhythmMinutes { get; set; }
        /// <summary>
        /// Соответствует критерию (≤ 2)
        /// </summary>
        public bool IsValidSyncRhythm { get; set; }
        #endregion

        #region LTV и STV
        /// <summary>
        ///  LTV уд/мин
        /// </summary>
        public float? BeatLTV { get; set; }
        /// <summary>
        ///  LTV мс
        /// </summary>
        public float? TimeMsLTV { get; set; }
        /// <summary>
        ///  STV, мс
        /// </summary>
        public float? STV { get; set; }
        /// <summary>
        /// Соответствует критерию
        /// </summary>
        public bool STVValid { get; set; }
        #endregion

        #region Осцилляции и шевеления
        /// <summary>
        /// Частота осцилляций, мс
        /// </summary>
        public float? OscillationFrequency { get; set; }
        /// <summary>
        /// Частота ШП, 1/ч
        /// </summary>
        public float? MovementFrequency { get; set; }
        /// <summary>
        /// Соответствует критерию (≥ 3)
        /// </summary>
        public bool MovementFrequencyValid { get; set; }
        #endregion

        public bool IsTimeDependentParameters { get; set; }

        public int CountRoodDawsonCriteriaValid()
        {
            var countValidCriteria = 0;

            if (IsValidRecordingDuration)
                countValidCriteria++;

            if (SignalLossValid)
                countValidCriteria++;

            if (BasalHeartRateValid)
                countValidCriteria++;

            if (DecelerationsMark) 
                countValidCriteria++;

            if (IsValidSyncRhythm)
                countValidCriteria++;

            if (STVValid)
                countValidCriteria++;
            
            if (MovementFrequencyValid)
                countValidCriteria++;

            if (IsTimeDependentParameters)
                countValidCriteria++;

            return countValidCriteria;
        }

        public bool IsRoodDawsonCriteriaValid()
        {
            return IsValidRecordingDuration
                && SignalLossValid
                && BasalHeartRateValid
                && DecelerationsMark
                && IsValidSyncRhythm
                && STVValid
                && MovementFrequencyValid
                && IsTimeDependentParameters;
        }
    }
}
