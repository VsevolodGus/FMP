using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using static CatAna;
using static CatAna.AnalysisResultUser;

namespace Bioss.Ultrasound.Services
{
    public class CatAnaService
    {
        /// <summary>
        /// CatAna требует для рассчетов чтобы 1 секунду состояла из 16 частей
        /// </summary>
        private const int TargetFrequency = 16;

        private readonly CatAna _catAna;
        private readonly InfoSettingsService _infoSettingsService;
        private readonly static UserCriterion marksUser = new UserCriterion()
        {
            recordLenMin = CardiograhyConstants.MinRecordingDuration,
            lossPercentMax = CardiograhyConstants.MaxSignalLossPercentage,

            basalRateMin = CardiograhyConstants.MinBasalHeartRate,
            basalRateMax = CardiograhyConstants.MaxBasalHeartRate,

            signDecMax = CardiograhyConstants.MaxCountDec,
            sinMax = CardiograhyConstants.AbsenseSynRhythm,
            stvMin = CardiograhyConstants.MinValueSTV,
            mphMin = CardiograhyConstants.MinMovementFrequency, //Минимальное значение частоты шевелений
            periodDependent = true //Срокозависимые параметры
        };


        public CatAnaService(InfoSettingsService infoSettingsService)
        {
            _catAna = new CatAna();
            _infoSettingsService = infoSettingsService;
        }

        /// <summary>
        /// Рассчет КТГ плода на текущий момент
        /// </summary>
        /// <param name="record">данные записи</param>
        /// <returns></returns>
        public CardiotocographyInfo CargiographAnalayzeWithUserSettings(Record record)
        {
            var heartRateResult = SignalSampler.Sampling<FhrData, float>(record.RecordingTimeSpan,
                record.StartTime,
                record.Fhrs,
                obj => obj.Time,
                obj => obj.Fhr,
                TargetFrequency,
                true);

            var movementsResult = SignalSampler.Sampling(record.RecordingTimeSpan,
                record.StartTime,
                record.Events,
                obj => obj.Time,
                obj => obj.Event == Events.FetalMovement,
                TargetFrequency);

            return CargiographAnalayzeWithUserSettings(_infoSettingsService.PregnancyWeek, heartRateResult, movementsResult);
        }

        /// <summary>
        /// Функция адаптер для CatAna
        /// </summary>
        /// <param name="pergnancyWeek">кол-во неделей беременности</param>
        /// <param name="heartRate">пульс, 1 сек = 16 записей</param>
        /// <param name="movement">движение плода, 1 сек = 16 записей</param>
        /// <returns>рассчеты по КТГ плода</returns>
        private CardiotocographyInfo CargiographAnalayzeWithUserSettings(int pergnancyWeek, float[] heartRate, bool[] movement)
        {
            var resultCtgUser = _catAna.analyseCtgUser(pergnancyWeek, heartRate, movement, marksUser);
            var analysisParams = resultCtgUser.analysisParams;
            return new CardiotocographyInfo()
            {
                RecordingDuration = analysisParams.recordLen,
                SignalLossPercentage = analysisParams.lossPercent,
                BasalHeartRate = analysisParams.basalRate,
                AccelerationsOver10 = analysisParams.acc10,
                AccelerationsOver15 = analysisParams.acc15,
                Decelerations = analysisParams.signDec,
                HighVariabilityMinutes = analysisParams.hv,
                LowVariabilityMinutes = analysisParams.lv,
                SyncRhythmMinutes = analysisParams.sin,
                TimeMsLTV = analysisParams.ltv,
                BeatLTV = analysisParams.ltvBpm,
                STV = analysisParams.stv,
                OscillationFrequency = analysisParams.ltf,
                MovementFrequency = analysisParams.mph,

                IsValidRecordingDuration = resultCtgUser.recordLenMark.IsCorrect(),
                SignalLossValid = resultCtgUser.lossPercentMark.IsCorrect(),
                BasalHeartRateValid = resultCtgUser.basalRateMark.IsCorrect(),
                DecelerationsMark = resultCtgUser.signDecMark.IsCorrect(),
                IsValidSyncRhythm = resultCtgUser.sinMark.IsCorrect(),
                STVValid = resultCtgUser.stvMark.IsCorrect(),
                MovementFrequencyValid = resultCtgUser.mphMark.IsCorrect(),
                IsTimeDependentParameters = resultCtgUser.periodDependentMark.IsCorrect(),
            };
        }
    }

    public static class ParamMarkExtension 
    {
        public static bool IsCorrect(this ParamMark paramMark)
            => paramMark == ParamMark.ok;
    }
}
