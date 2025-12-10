using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Tools;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System;

namespace Bioss.Ultrasound.Services.Extensions
{
    public static class XGraphicsExtensions
    {
        public static void DrawHeader(this XGraphics graphics,
            PdfPage page,
            string hospital,
            DateTime dateOfResearch,
            string patient,
            string doctor,
            DateTime? pregnancyDate,
            DateTime defaultPregnancyDate
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
            settings.DrawString(PdfOrderConstants.HeaderFontSize, 3, XStringAlignment.Center, GetStringPergnancyTime(pregnancyDate, defaultPregnancyDate));

            // отображение левых элементов
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 2, XStringAlignment.Far, AppStrings.PDF_HeaderDoctor);
            settings.DrawString(PdfOrderConstants.HeaderFontSize, 3, XStringAlignment.Far, doctor);
        }

        private static string GetStringPergnancyTime(DateTime? pergnancyTime, DateTime defaultPregnancyDate)
        {
            var (weeks, days) = pergnancyTime.HasValue
                ? pergnancyTime.Value.CalculatePregnantTime()
                : defaultPregnancyDate.CalculatePregnantTime();

            var prefix = pergnancyTime.HasValue
                ? AppStrings.PDF_GestationalAge
                : AppStrings.PDF_GestationalAgeByDefault;

            return $"{prefix} {weeks} / {days}";
        }

        public static void DrawData(this XGraphics gfx,
            PdfPage page,
            Biometric biometric,
            TimeSpan recordTime,
            int fetalsCount,
            DateTime dateOfResearch,
            DateTime? pregnancyStart)
        {
            biometric ??= new Biometric();

            var settings = new DrawStringSettings
            {
                Page = page,
                Graphics = gfx,
                LineHeight = XUnit.FromMillimeter(4.5),
                PaddingTop = XUnit.FromMillimeter(25),
                PaddingLeft = XUnit.FromMillimeter(10),
                PaddingRight = XUnit.FromMillimeter(10),
            };

            settings.DrawString(PdfOrderConstants.DefaultFontSize, 0, XStringAlignment.Near, $"{AppStrings.PDF_RecordingDuration}: {recordTime.Minutes} {AppStrings.Unit_min}");
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 1, XStringAlignment.Near, $"{AppStrings.PDF_FetalMovements}: {fetalsCount}");
            if (pregnancyStart.HasValue)
            {
                var time = pregnancyStart.Value.CalculatePregnantTime(dateOfResearch);
                settings.DrawString(PdfOrderConstants.DefaultFontSize, 2, XStringAlignment.Near, $"{AppStrings.PDF_GestationalAge}: {time.weeks}/{time.days}");

            }

            settings.PaddingLeft = XUnit.FromMillimeter(60);
            var empty = PdfOrderConstants.DefaultValue;    
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 0, XStringAlignment.Near, $"{AppStrings.PDF_Temperature}: {biometric.Temperature.ToStringOrEmptyString(empty)}{AppStrings.Unit_Celsius}");
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 1, XStringAlignment.Near, $"{AppStrings.PDF_Pulse}: {biometric.Pulse.ToStringOrEmptyString(empty)} {AppStrings.Unit_HeartRate}");
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 2, XStringAlignment.Near, $"{AppStrings.PDF_Sugar}: {biometric.Sugar.ToStringOrEmptyString(empty)} {AppStrings.Unit_BloodGlucose}");
            settings.DrawString(PdfOrderConstants.DefaultFontSize, 3, XStringAlignment.Near, $"{AppStrings.PDF_HeartRate}: {biometric.Systolic.ToStringOrEmptyString(empty)}/{biometric.Diastolic.ToStringOrEmptyString(empty)} {AppStrings.Unit_MillimetreOfMercury}");

            settings.PaddingLeft = XUnit.FromMillimeter(60 + 45);
            settings.LineHeight = XUnit.FromMillimeter(4.5);

            settings.DrawMultilineString(PdfOrderConstants.DefaultFontSize, 0, 2, $"{AppStrings.PDF_Comment}: {biometric.Comment}");
            settings.DrawMultilineString(PdfOrderConstants.DefaultFontSize, 2, 2, $"{AppStrings.PDF_DoctorsConclusion}:");
        }

        public static void DrawPageNumbers(this XGraphics gfx,
            PdfPage page,
            int pageNumber,
            int pageCount)
        {
            var lineHeight = XUnit.FromMillimeter(2.5);
            var padding = XUnit.FromMillimeter(page.Height.Millimeter - 5);
            var paddingLeft = XUnit.FromMillimeter(10);
            gfx.DrawString(string.Format(AppStrings.PDF_FooterPageNumbers, pageNumber, pageCount), page, padding, paddingLeft, 10, lineHeight, 0, XStringAlignment.Far);
        }

        public static void DrawDeviceSerialNumber(this XGraphics gfx, PdfPage page, string serialNumber)
        {
            var lineHeight = XUnit.FromMillimeter(2.5);
            var padding = XUnit.FromMillimeter(page.Height.Millimeter - 5);
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
}
