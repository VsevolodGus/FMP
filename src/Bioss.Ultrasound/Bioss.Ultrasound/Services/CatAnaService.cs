using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Tools;
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

        private readonly CatAna _catAna = new CatAna();
        private readonly static UserCriterion marksUser = new UserCriterion()
        {
            recordLenMin = CardiograhyConstants.MinRecordingDuration,
            basalRateMin = CardiograhyConstants.MinBasalHeartRate,
            basalRateMax = CardiograhyConstants.MaxBasalHeartRate,
            lossPercentMax = CardiograhyConstants.MaxSignalLossPercentage,
            decMax = CardiograhyConstants.MaxCountDec,
            sinMax = CardiograhyConstants.AbsenseSynRhythm,
            stvMin = CardiograhyConstants.MinValueSTV,
            mphMin = CardiograhyConstants.MinMovementFrequency, //Минимальное значение частоты шевелений
            periodDependent = true //Срокозависимые параметры
        };

        public CardiotocographyInfo CargiographAnalayzeWithUserSettings(DateTime pergnancyDate, Record record)
        {
            var heartRateResult = ConvertToArray<FhrData, float>(record.RecordingTimeSpan, 
                record.StartTime, 
                record.Fhrs, 
                obj => obj.Time, 
                obj => obj.Fhr);

            var movementsResult = ConvertToArray(record.RecordingTimeSpan,
                record.StartTime,
                record.Events,
                obj => obj.Time,
                obj => obj.Event == Events.FetalMovement);

            return CargiographAnalayzeWithUserSettings(pergnancyDate.CalculatePregnantTime().weeks, heartRateResult, movementsResult);
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
        /// <returns></returns>
        private TResultItem[] ConvertToArray<TObject, TResultItem>(TimeSpan duration,
            DateTime startDate,
            IEnumerable<TObject> items,
            Func<TObject, DateTime> getTime,
            Func<TObject, TResultItem> getValue)
        {
            var totalSeconds = duration.TotalSeconds;
            var arrayLength = (int)Math.Ceiling(totalSeconds * CountItemInSecond);
            var result = new TResultItem[arrayLength];

            foreach (var item in items)
            {
                TimeSpan offset = getTime(item) - startDate;
                double secondsFromStart = offset.TotalSeconds;
                int index = (int)Math.Round(secondsFromStart * CountItemInSecond);

                if (index == arrayLength)
                    index = arrayLength - 1;
                else if (index < 0 || index >= arrayLength)
                    continue;

                result[index] = getValue(item);
            }

            return result;
        }

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
                BeatLTV = analysisParams.ltvBpm,
                TimeMsLTV = analysisParams.ltv,
                STV = analysisParams.stv,
                OscillationFrequency = analysisParams.ltf,
                MovementFrequency = analysisParams.mph,
               
                SignalLossValid = resultCtgUser.signDecMark.IsCorrect(),
                IsValidRecordingDuration = resultCtgUser.recordLenMark.IsCorrect(),
                BasalHeartRateValid = resultCtgUser.basalRateMark.IsCorrect(),
                DecelerationsMark = resultCtgUser.decMark.IsCorrect(),
                IsValidSyncRhythm = resultCtgUser.decMark.IsCorrect(),
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
