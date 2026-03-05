namespace Bioss.Ultrasound.Core.Services.Constants;

internal struct PdfOrderConstants
{
    /// <summary>
    /// Размер строки в таблице
    /// </summary>
    public const string SizeTableRow = "0.05cm";
    /// <summary>
    /// Ширина A4
    /// </summary>
    public const string WidthA4 = "28cm";
    /// <summary>
    /// Название шрифта для отчета
    /// </summary>
    public const string FontName = "Segoe UI";

    /// <summary>
    /// Знак о не прохождение условий
    /// </summary>
    public const string No = "X";
    /// <summary>
    /// Знак о прохождение условий
    /// </summary>
    public const string Yes = "V";
    /// <summary>
    /// Стандартный размер шрифта для ПДФ отчета, не считая графика
    /// </summary>
    public const int DefaultFontSize = 9;
    /// <summary>
    /// Стандартный размер шрифта для графика в ПДФ отчете
    /// </summary>
    public const int FontSizeGrafic = 8;
    /// <summary>
    /// Стандратный размер шрифта для заголовков
    /// </summary>
    public const int HeaderFontSize = 12;
    /// <summary>
    /// Дефолтное Значение для параметров, значение которых не получено. Если параметры отображаются одним числом
    /// </summary>
    public const string DefaultValue = "-";
    /// <summary>
    /// Дефолтное Значение для параметров, значение которых не получено. Если параметры отображаются двумя числами
    /// </summary>
    public const string DefaultDoubleValue = "-(-)";
}
