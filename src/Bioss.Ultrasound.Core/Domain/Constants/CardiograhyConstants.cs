namespace Bioss.Ultrasound.Core.Domain.Constants;

public struct CardiograhyConstants
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
    public const int MinValueSTV = 4;

    /// <summary>
    /// Минимальное кол-во движений плода
    /// </summary>
    public const int MinMovementFrequency = 3;

    public const int AbsenseSynRhythm = 2;
    public const int MaxCountDec = 0;

    /// <summary>
    /// Минимально допустимое кол-во недель пациента
    /// </summary>
    public const int MinPregnantWeeks = 24;
    /// <summary>
    /// Максильно допустимое кол-во недель пациента
    /// </summary>
    public const int MaxPregnantWeeks = 40;
    /// <summary>
    /// Граница сроко-засимых параметров
    /// </summary>
    public const int BoundaryWeekOfTimeDependentParameters = 28;
}
