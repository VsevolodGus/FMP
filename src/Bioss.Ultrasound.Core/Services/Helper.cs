using Bioss.Ultrasound.Core.Services.Constants;
using Bioss.Ultrasound.Core.Services.Extensions;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bioss.Ultrasound.Core.Services;

public static class Helper
{
    public static void DrawString(this XGraphics gfx, string text, PdfPage page, XUnit paddingTop, XUnit paddingLeft, int fontSize, XUnit lineHeight, int lineNumber, XStringAlignment textAlignment, XFontStyle fontStyle = XFontStyle.Regular)
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
        var font = new XFont(PdfOrderConstants.FontName, fontSize, fontStyle);
        gfx.DrawString(text, font, XBrushes.Black, rect, format);
    }

    public static void DrawString(this DrawStringSettings settings, int fontSize, int lineNumber, XStringAlignment textAlignment, string text)
    {
        var heightPoints = settings.LineHeight.Point;
        var paddingTopPoints = settings.PaddingTop.Point;
        var paddingLeftPoints = settings.PaddingLeft.Point;
        var paddingRightPoints = settings.PaddingRight.Point;
        var widthPoints = settings.Page.Width.Point - paddingLeftPoints - paddingRightPoints;

        var yPoints = paddingTopPoints + heightPoints * lineNumber;
        var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

        var format = new XStringFormat
        {
            LineAlignment = XLineAlignment.Center,
            Alignment = textAlignment
        };
        var font = new XFont(PdfOrderConstants.FontName, fontSize);
        settings.Graphics.DrawString(text, font, XBrushes.Black, rect, format);
    }
}
