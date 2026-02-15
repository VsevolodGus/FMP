using Bioss.Ultrasound.Core.Services.Constants;
using Bioss.Ultrasound.Core.Tools;
using Bioss.Ultrasound.Resources.Localization;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bioss.Ultrasound.Core.Services.Extensions;

public static class XGraphicsExtensions
{
    /// <summary>
    /// Отрисовка заголовков отчета
    /// </summary>
    /// <param name="graphics">графика отчета</param>
    /// <param name="page">страница отчета</param>
    /// <param name="hospital">имя организации где происходит замер</param>
    /// <param name="dateOfResearch">дата исследования</param>
    /// <param name="patient">имя пациента</param>
    /// <param name="doctor">имя докотора</param>
    /// <param name="pregnancyWeek">неделя беременности</param>
    /// <param name="pregnancyDay">день беременности = N дней беременности % 7</param>
    public static void DrawHeader(this XGraphics graphics,
        PdfPage page,
        string hospital,
        DateTime dateOfResearch,
        string patient,
        string doctor,
        int pregnancyWeek,
        int pregnancyDay
        )
    {
        var settings = new DrawStringSettings
        {
            Page = page,
            Graphics = graphics,
            LineHeight = XUnit.FromMillimeter(4),
            PaddingTop = XUnit.FromMillimeter(5),
            PaddingLeft = XUnit.FromMillimeter(10),
            PaddingRight = XUnit.FromMillimeter(10),
        };

        var imageSource = MigraDocTools.GetImageSourceFromResources("bipuls_logo", "jpg");
        XImage image = XImage.FromImageSource(imageSource);
        graphics.DrawImage(image, settings.PaddingLeft.Point, settings.PaddingTop.Point, 22, 22);

        var tmpPaddingLeft = settings.PaddingLeft;
        settings.PaddingLeft = XUnit.FromMillimeter(settings.PaddingLeft.Millimeter + 10);

        // отображение правых элементов
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 0, XStringAlignment.Near, AppStrings.PDF_HeaderLeftCorner);
        settings.PaddingLeft = tmpPaddingLeft;

        settings.DrawString(PdfOrderConstants.DefaultFontSize, 2, XStringAlignment.Near, AppStrings.PDF_HeaderDateOfResearch);
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 3, XStringAlignment.Near, $"{dateOfResearch}");

        // Отображение центральных элементов
        settings.DrawString(PdfOrderConstants.HeaderFontSize, -1, XStringAlignment.Center, $"{hospital}");
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 0, XStringAlignment.Center, string.Empty);
        settings.DrawString(PdfOrderConstants.DefaultFontSize, 1, XStringAlignment.Center, AppStrings.PDF_HeaderPatient);
        
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 2, XStringAlignment.Center, $"{patient}");
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 3, XStringAlignment.Center, $"{AppStrings.PDF_GestationalAge} {pregnancyWeek} / {pregnancyDay}");

        // отображение левых элементов
        settings.DrawString(PdfOrderConstants.DefaultFontSize, 2, XStringAlignment.Far, AppStrings.PDF_HeaderDoctor);
        settings.DrawString(PdfOrderConstants.HeaderFontSize, 3, XStringAlignment.Far, doctor);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gfx"></param>
    /// <param name="page"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageCount"></param>
    public static void DrawPageNumbers(this XGraphics gfx,
        PdfPage page,
        int pageNumber,
        int pageCount)
    {
        var lineHeight = XUnit.FromMillimeter(2.5);
        var padding = XUnit.FromMillimeter(page.Height.Millimeter - 10);
        var paddingLeft = XUnit.FromMillimeter(10);
        gfx.DrawString(string.Format(AppStrings.PDF_FooterPageNumbers, pageNumber, pageCount), page, padding, paddingLeft, 10, lineHeight, 0, XStringAlignment.Far);
    }

    /// <summary>
    /// Настройка отрисовки данных по датчику в ПДФ отчете
    /// </summary>
    /// <param name="gfx">графика для ПДФ отчета</param>
    /// <param name="page">Страница ПДФ отчета</param>
    /// <param name="serialNumber">серийный номер датчика</param>
    public static void DrawDeviceSerialNumber(this XGraphics gfx, PdfPage page, string serialNumber)
    {
        var lineHeight = XUnit.FromMillimeter(2.5);
        var padding = XUnit.FromMillimeter(page.Height.Millimeter - 10);
        var paddingLeft = XUnit.FromMillimeter(10);
        gfx.DrawString(string.Format(AppStrings.PDF_FooterSN, serialNumber), page, padding, paddingLeft, 10, lineHeight, 0, XStringAlignment.Near);
    }

    public static void DrawString(this XGraphics gfx, string text, PdfPage page, XUnit paddingTop, XUnit paddingLeft, int fontSize, XUnit lineHeight, int lineNumber, XStringAlignment textAlignment)
    {
        var heightPoints = lineHeight.Point;
        var paddingTopPoints = paddingTop.Point;
        var paddingLeftPoints = paddingLeft.Point;
        var widthPoints = page.Width.Point - paddingLeftPoints * 2;

        var yPoints = paddingTopPoints + heightPoints * lineNumber;
        var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

        var format = new XStringFormat
        {
            LineAlignment = XLineAlignment.Center,
            Alignment = textAlignment
        };
        var font = new XFont(PdfOrderConstants.FontName, fontSize);
        var brush = XBrushes.Black;
        gfx.DrawString(text, font, brush, rect, format);
    }
}

public class DrawStringSettings
{
    public PdfPage Page { get; set; }
    public XGraphics Graphics { get; set; }
    public XUnit PaddingTop { get; set; }
    public XUnit PaddingLeft { get; set; }
    public XUnit PaddingRight { get; set; }
    public XUnit LineHeight { get; set; }
}
