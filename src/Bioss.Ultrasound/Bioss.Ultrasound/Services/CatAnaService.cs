using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using System;
using System.Collections.Generic;
using static CatAna;
using static CatAna.AnalysisResultUser;

namespace Bioss.Ultrasound.Services
{
    public class CatAnaService
    {
        /// <summary>
        /// CatAna требует для рассчетов чтобы 1 секунду состояла из 16 частей
        /// </summary>
        private const byte CountItemInSecond = 16;

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
            var heartRateResult = ConvertToArray<FhrData, float>(record.RecordingTimeSpan,
                record.StartTime,
                record.Fhrs,
                obj => obj.Time,
                obj => obj.Fhr,
                true);

            var movementsResult = ConvertToArray(record.RecordingTimeSpan,
                record.StartTime,
                record.Events,
                obj => obj.Time,
                obj => obj.Event == Events.FetalMovement);

            return CargiographAnalayzeWithUserSettings(_infoSettingsService.PregnancyWeek, heartRateResult, movementsResult);
        }

        /// <summary>
        /// Конвертация в массив для CatAna
        /// </summary>
        /// <typeparam name="TObject">объект для которого идет рассчет</typeparam>
        /// <typeparam name="TResultItem">результат рассчета</typeparam>
        /// <param name="duration">время записи</param>
        /// <param name="startDate">начало записи</param>
        /// <param name="items">элементы которые конвертируются для рассчета</param>
        /// <param name="getTime">метод получения времени из TObject</param>
        /// <param name="getValue">метод получения значения из TObject</param>
        /// <param name="isSampler">нужно ли заполнять пустоты между значениями или нет. Заполненяет пустоты от предыдущего до текущего элемента, значением текущего элемента</param>
        /// <returns></returns>
        private TResultItem[] ConvertToArray<TObject, TResultItem>(TimeSpan duration,
            DateTime startDate,
            IEnumerable<TObject> items,
            Func<TObject, DateTime> getTime,
            Func<TObject, TResultItem> getValue,
            bool isSampler = false)
        {
            var totalSeconds = duration.TotalSeconds;
            var arrayLength = (int)Math.Ceiling(totalSeconds * CountItemInSecond);
            var result = new TResultItem[arrayLength];
            var lastIndex = 0;
            foreach (var item in items)
            {
                TimeSpan offset = getTime(item) - startDate;
                double secondsFromStart = offset.TotalSeconds;
                int index = (int)Math.Round(secondsFromStart * CountItemInSecond);

                if (index == arrayLength)
                    index = arrayLength - 1;
                else if (index < 0 || index >= arrayLength)
                    continue;

                var currentValue = getValue(item);
                result[index] = currentValue;
                

                if (!isSampler)
                    continue;

                for (var i = lastIndex + 1; i < index; i++)
                    result[i] = currentValue;
                lastIndex = index;
            }

            return result;
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
