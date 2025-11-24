using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using static CatAna;
using static CatAna.AnalysisResultUser;

namespace Bioss.Ultrasound.Services
{
    public class CatAnaService
    {
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
            mphMin = CardiograhyConstants.MinMovementFrequency,
            periodDependent = true
        };

        public CardiotocographyInfo CargiographAnalayzeWithUserSettings(int pergnancyWeek, float[] heartRate, bool[] movement)
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
