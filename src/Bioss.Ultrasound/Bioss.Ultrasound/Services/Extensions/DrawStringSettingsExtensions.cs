using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Drawing;

namespace Bioss.Ultrasound.Services.Extensions
{
    internal static class DrawStringSettingsExtensions
    {
        public static void DrawString(this DrawStringSettings settings, 
            int fontSize, 
            int lineNumber, 
            XStringAlignment textAlignment, 
            string text)
        {
            var heightPoints = settings.LineHeight.Point;
            var paddingTopPoints = settings.PaddingTop.Point;
            var paddingLeftPoints = settings.PaddingLeft.Point;
            var paddingRightPoints = settings.PaddingRight.Point;
            var widthPoints = settings.Page.Width.Point - paddingLeftPoints - paddingRightPoints;

            var yPoints = paddingTopPoints + (heightPoints * lineNumber);
            var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

            var format = new XStringFormat
            {
                LineAlignment = XLineAlignment.Center,
                Alignment = textAlignment
            };
            var font = new XFont(PdfOrderConstants.FontName, fontSize);
            var brush = XBrushes.Black;
            settings.Graphics.DrawString(text, font, brush, rect, format);
        }

        public static void DrawMultilineString(this DrawStringSettings settings, int fontSize, int lineNumber, int lineCount, string text)
        {
            var lineHeight = settings.LineHeight.Point;
            var heightPoints = lineHeight * lineCount;
            var paddingTopPoints = settings.PaddingTop.Point;
            var paddingLeftPoints = settings.PaddingLeft.Point;
            var paddingRightPoints = settings.PaddingRight.Point;
            var widthPoints = settings.Page.Width.Point - paddingLeftPoints - paddingRightPoints;

            var yPoints = paddingTopPoints + (lineHeight * lineNumber);
            var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

            var font = new XFont(PdfOrderConstants.FontName, fontSize);
            var brush = XBrushes.Black;

            var formatter = new XTextFormatter(settings.Graphics);
            formatter.DrawString(text, font, brush, rect, XStringFormats.TopLeft);
        }
    }
}
