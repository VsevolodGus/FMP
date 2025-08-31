using System;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bioss.Ultrasound.Tools.PdfTests
{
    //http://www.pdfsharp.net/wiki/Graphics-sample.ashx

    public class PdfSharpDrawingTest : IPdfTest
    {
        public string Name => "PdfSharp - Drawing";

        public void CreatePdfFile(string fileName)
        {
            MyFontResolver.Apply();

            var document = new PdfDocument();
            document.Info.Title = "PDFsharp XGraphic Sample";
            document.Info.Author = "Stefan Lange";
            document.Info.Subject = "Created with code snippets that show the use of graphical functions";
            document.Info.Keywords = "PDFsharp, XGraphics";

            // Create demonstration pages
            new LinesAndCurves().DrawPage(document, document.AddPage());
            new Shapes().DrawPage(document, document.AddPage());
            new Paths().DrawPage(document, document.AddPage());
            new Text().DrawPage(document, document.AddPage());

            // Save the s_document...
            document.Save(fileName);
        }
    }

    class DrawHelper
    {
        private static int borderWidth = 2;
        public static string FontName = "Segoe UI";

        public static void DrawTitle(PdfDocument s_document, PdfPage page, XGraphics gfx, string title)
        {
            XRect rect = new XRect(new XPoint(), gfx.PageSize);
            rect.Inflate(-10, -15);
            XFont font = new XFont(FontName, 14, XFontStyle.Bold);
            gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);

            rect.Offset(0, 5);
            font = new XFont(FontName, 8, XFontStyle.Italic);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Far;
            gfx.DrawString("Created with " + "dsdsa", font, XBrushes.DarkOrchid, rect, format);

            font = new XFont(FontName, 8);
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(s_document.PageCount.ToString(), font, XBrushes.DarkOrchid, rect, format);

            s_document.Outlines.Add(title, page, true);
        }

        public static void BeginBox(XGraphics gfx, int number, string title)
        {
            const int dEllipse = 15;
            XRect rect = new XRect(0, 20, 300, 200);
            if (number % 2 == 0)
                rect.X = 300 - 5;
            rect.Y = 40 + ((number - 1) / 2) * (200 - 5);
            rect.Inflate(-10, -10);
            XRect rect2 = rect;
            rect2.Offset(borderWidth, borderWidth);

            var shadowColor = XColors.Blue;
            var backColor = XColors.Red;
            var backColor2 = XColors.Green;
            var borderPen = new XPen(XColors.Black);

            gfx.DrawRoundedRectangle(new XSolidBrush(shadowColor), rect2, new XSize(dEllipse + 8, dEllipse + 8));
            XLinearGradientBrush brush = new XLinearGradientBrush(rect, backColor, backColor2, XLinearGradientMode.Vertical);
            gfx.DrawRoundedRectangle(borderPen, brush, rect, new XSize(dEllipse, dEllipse));
            rect.Inflate(-5, -5);

            XFont font = new XFont(FontName, 12, XFontStyle.Regular);
            gfx.DrawString(title, font, XBrushes.Navy, rect, XStringFormats.TopCenter);

            rect.Inflate(-10, -5);
            rect.Y += 20;
            rect.Height -= 20;

            var state = gfx.Save();
            gfx.TranslateTransform(rect.X, rect.Y);
        }

        public static void EndBox(XGraphics gfx)
        {
            // gfx.Restore(this.state);
            gfx.Restore();
        }
    }

    class LinesAndCurves
    {
        public void DrawPage(PdfDocument document, PdfPage page)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHelper.DrawTitle(document, page, gfx, "Lines & Curves");

            DrawLine(gfx, 1);
            DrawLines(gfx, 2);
            DrawBezier(gfx, 3);
            DrawBeziers(gfx, 4);
            DrawCurve(gfx, 5);
            DrawArc(gfx, 6);
        }

        void DrawLine(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawLine");

            gfx.DrawLine(XPens.DarkGreen, 0, 0, 250, 0);

            gfx.DrawLine(XPens.Gold, 15, 7, 230, 15);

            XPen pen = new XPen(XColors.Navy, 4);
            gfx.DrawLine(pen, 0, 20, 250, 20);

            pen = new XPen(XColors.Firebrick, 6);
            pen.DashStyle = XDashStyle.Dash;
            gfx.DrawLine(pen, 0, 40, 250, 40);
            pen.Width = 7.3;
            pen.DashStyle = XDashStyle.DashDotDot;
            gfx.DrawLine(pen, 0, 60, 250, 60);

            pen = new XPen(XColors.Goldenrod, 10);
            pen.LineCap = XLineCap.Flat;
            gfx.DrawLine(pen, 10, 90, 240, 90);
            gfx.DrawLine(XPens.Black, 10, 90, 240, 90);

            pen = new XPen(XColors.Goldenrod, 10);
            pen.LineCap = XLineCap.Square;
            gfx.DrawLine(pen, 10, 110, 240, 110);
            gfx.DrawLine(XPens.Black, 10, 110, 240, 110);

            pen = new XPen(XColors.Goldenrod, 10);
            pen.LineCap = XLineCap.Round;
            gfx.DrawLine(pen, 10, 130, 240, 130);
            gfx.DrawLine(XPens.Black, 10, 130, 240, 130);

            DrawHelper.EndBox(gfx);
        }

        void DrawLines(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawLines");

            XPen pen = new XPen(XColors.DarkSeaGreen, 6);
            pen.LineCap = XLineCap.Round;
            pen.LineJoin = XLineJoin.Bevel;
            XPoint[] points =
              new XPoint[] { new XPoint(20, 30), new XPoint(60, 120), new XPoint(90, 20), new XPoint(170, 90), new XPoint(230, 40) };
            gfx.DrawLines(pen, points);

            DrawHelper.EndBox(gfx);
        }

        void DrawBezier(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawBezier");

            gfx.DrawBezier(new XPen(XColors.DarkRed, 5), 20, 110, 40, 10, 170, 90, 230, 20);

            DrawHelper.EndBox(gfx);
        }

        void DrawBeziers(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawBeziers");

            XPoint[] points = new XPoint[]{new XPoint(20, 30), new XPoint(40, 120), new XPoint(80, 20), new XPoint(110, 90),
                                 new XPoint(180, 40), new XPoint(210, 40), new XPoint(220, 80)};
            XPen pen = new XPen(XColors.Firebrick, 4);
            gfx.DrawBeziers(pen, points);

            DrawHelper.EndBox(gfx);
        }

        void DrawCurve(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawCurve");

            XPoint[] points =
              new XPoint[] { new XPoint(20, 30), new XPoint(60, 120), new XPoint(90, 20), new XPoint(170, 90), new XPoint(230, 40) };
            XPen pen = new XPen(XColors.RoyalBlue, 3.5);
            gfx.DrawCurve(pen, points, 1);

            DrawHelper.EndBox(gfx);
        }

        void DrawArc(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawArc");

            XPen pen = new XPen(XColors.Plum, 4.7);
            gfx.DrawArc(pen, 0, 0, 250, 140, 190, 200);

            DrawHelper.EndBox(gfx);
        }
    }

    class Shapes
    {
        public void DrawPage(PdfDocument document, PdfPage page)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHelper.DrawTitle(document, page, gfx, "Shapes");

            DrawRectangle(gfx, 1);
            DrawRoundedRectangle(gfx, 2);
            DrawEllipse(gfx, 3);
            //DrawPolygon(gfx, 4);
            DrawPie(gfx, 5);
            DrawClosedCurve(gfx, 6);
        }

        void DrawRectangle(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawRectangle");

            XPen pen = new XPen(XColors.Navy, Math.PI);

            gfx.DrawRectangle(pen, 10, 0, 100, 60);
            gfx.DrawRectangle(XBrushes.DarkOrange, 130, 0, 100, 60);
            gfx.DrawRectangle(pen, XBrushes.DarkOrange, 10, 80, 100, 60);
            gfx.DrawRectangle(pen, XBrushes.DarkOrange, 150, 80, 60, 60);

            DrawHelper.EndBox(gfx);
        }

        void DrawRoundedRectangle(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawRoundedRectangle");

            XPen pen = new XPen(XColors.RoyalBlue, Math.PI);

            gfx.DrawRoundedRectangle(pen, 10, 0, 100, 60, 30, 20);
            gfx.DrawRoundedRectangle(XBrushes.Orange, 130, 0, 100, 60, 30, 20);
            gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 10, 80, 100, 60, 30, 20);
            gfx.DrawRoundedRectangle(pen, XBrushes.Orange, 150, 80, 60, 60, 20, 20);

            DrawHelper.EndBox(gfx);
        }

        void DrawEllipse(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawEllipse");

            XPen pen = new XPen(XColors.DarkBlue, 2.5);

            gfx.DrawEllipse(pen, 10, 0, 100, 60);
            gfx.DrawEllipse(XBrushes.Goldenrod, 130, 0, 100, 60);
            gfx.DrawEllipse(pen, XBrushes.Goldenrod, 10, 80, 100, 60);
            gfx.DrawEllipse(pen, XBrushes.Goldenrod, 150, 80, 60, 60);

            DrawHelper.EndBox(gfx);
        }

        //void DrawPolygon(XGraphics gfx, int number)
        //{
        //    DrawHelper.BeginBox(gfx, number, "DrawPolygon");

        //    XPen pen = new XPen(XColors.DarkBlue, 2.5);

        //    gfx.DrawPolygon(pen, XBrushes.LightCoral, GetPentagram(50, new XPoint(60, 70)), XFillMode.Winding);
        //    gfx.DrawPolygon(pen, XBrushes.LightCoral, GetPentagram(50, new XPoint(180, 70)), XFillMode.Alternate);

        //    DrawHelper.EndBox(gfx);
        //}

        void DrawPie(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawPie");

            XPen pen = new XPen(XColors.DarkBlue, 2.5);

            gfx.DrawPie(pen, 10, 0, 100, 90, -120, 75);
            gfx.DrawPie(XBrushes.Gold, 130, 0, 100, 90, -160, 150);
            gfx.DrawPie(pen, XBrushes.Gold, 10, 50, 100, 90, 80, 70);
            gfx.DrawPie(pen, XBrushes.Gold, 150, 80, 60, 60, 35, 290);

            DrawHelper.EndBox(gfx);
        }

        void DrawClosedCurve(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawClosedCurve");

            XPen pen = new XPen(XColors.DarkBlue, 2.5);
            gfx.DrawClosedCurve(pen, XBrushes.SkyBlue,
              new XPoint[] { new XPoint(10, 120), new XPoint(80, 30), new XPoint(220, 20), new XPoint(170, 110), new XPoint(100, 90) },
              XFillMode.Winding, 0.7);

            DrawHelper.EndBox(gfx);
        }
    }

    class Paths
    {
        public void DrawPage(PdfDocument document, PdfPage page)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHelper.DrawTitle(document, page, gfx, "Paths");

            DrawPathOpen(gfx, 1);
            DrawPathClosed(gfx, 2);
            DrawPathAlternateAndWinding(gfx, 3);
            DrawGlyphs(gfx, 5);
            DrawClipPath(gfx, 6);
        }

        void DrawPathOpen(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawPath (open)");

            XPen pen = new XPen(XColors.Navy, Math.PI);
            pen.DashStyle = XDashStyle.Dash;

            XGraphicsPath path = new XGraphicsPath();
            path.AddLine(10, 120, 50, 60);
            path.AddArc(50, 20, 110, 80, 180, 180);
            path.AddLine(160, 60, 220, 100);
            gfx.DrawPath(pen, path);

            DrawHelper.EndBox(gfx);
        }

        void DrawPathClosed(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawPath (closed)");

            XPen pen = new XPen(XColors.Navy, Math.PI);
            pen.DashStyle = XDashStyle.Dash;

            XGraphicsPath path = new XGraphicsPath();
            path.AddLine(10, 120, 50, 60);
            path.AddArc(50, 20, 110, 80, 180, 180);
            path.AddLine(160, 60, 220, 100);
            path.CloseFigure();
            gfx.DrawPath(pen, path);

            DrawHelper.EndBox(gfx);
        }

        void DrawPathAlternateAndWinding(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "DrawPath (alternate / winding)");

            XPen pen = new XPen(XColors.Navy, 2.5);

            // Alternate fill mode
            XGraphicsPath path = new XGraphicsPath();
            path.FillMode = XFillMode.Alternate;
            path.AddLine(10, 130, 10, 40);
            path.AddBeziers(new XPoint[]{new XPoint(10, 40), new XPoint(30, 0), new XPoint(40, 20), new XPoint(60, 40),
                               new XPoint(80, 60), new XPoint(100, 60), new XPoint(120, 40)});
            path.AddLine(120, 40, 120, 130);
            path.CloseFigure();
            path.AddEllipse(40, 80, 50, 40);
            gfx.DrawPath(pen, XBrushes.DarkOrange, path);

            // Winding fill mode
            path = new XGraphicsPath();
            path.FillMode = XFillMode.Winding;
            path.AddLine(130, 130, 130, 40);
            path.AddBeziers(new XPoint[]{new XPoint(130, 40), new XPoint(150, 0), new XPoint(160, 20), new XPoint(180, 40),
                               new XPoint(200, 60), new XPoint(220, 60), new XPoint(240, 40)});
            path.AddLine(240, 40, 240, 130);
            path.CloseFigure();
            path.AddEllipse(160, 80, 50, 40);
            gfx.DrawPath(pen, XBrushes.DarkOrange, path);

            DrawHelper.EndBox(gfx);
        }

        void DrawGlyphs(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "Draw Glyphs");

            XGraphicsPath path = new XGraphicsPath();
            path.AddString("Hello!", new XFontFamily(DrawHelper.FontName), XFontStyle.BoldItalic, 100, new XRect(0, 0, 250, 140),
              XStringFormats.Center);

            gfx.DrawPath(new XPen(XColors.Purple, 2.3), XBrushes.DarkOrchid, path);

            DrawHelper.EndBox(gfx);
        }

        void DrawClipPath(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "Clip through Path");

            XGraphicsPath path = new XGraphicsPath();
            path.AddString("Clip!", new XFontFamily(DrawHelper.FontName), XFontStyle.Bold, 90, new XRect(0, 0, 250, 140),
              XStringFormats.Center);

            gfx.IntersectClip(path);

            // Draw a beam of dotted lines
            XPen pen = XPens.DarkRed.Clone();
            pen.DashStyle = XDashStyle.Dot;
            for (double r = 0; r <= 90; r += 0.5)
                gfx.DrawLine(pen, 0, 0, 250 * Math.Cos(r / 90 * Math.PI), 250 * Math.Sin(r / 90 * Math.PI));

            DrawHelper.EndBox(gfx);
        }
    }

    class Text
    {
        public void DrawPage(PdfDocument document, PdfPage page)
        {
            XGraphics gfx = XGraphics.FromPdfPage(page);

            DrawHelper.DrawTitle(document, page, gfx, "Text");

            DrawText(gfx, 1);
            DrawTextAlignment(gfx, 2);
            MeasureText(gfx, 3);
        }

        void DrawText(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "Text Styles");

            string facename = DrawHelper.FontName;

            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.WinAnsi);

            XFont fontRegular = new XFont(facename, 20, XFontStyle.Regular, options);
            XFont fontBold = new XFont(facename, 20, XFontStyle.Bold, options);
            XFont fontItalic = new XFont(facename, 20, XFontStyle.Italic, options);
            XFont fontBoldItalic = new XFont(facename, 20, XFontStyle.BoldItalic, options);

            // The default alignment is baseline left (that differs from GDI+)
            gfx.DrawString("Times (regular)", fontRegular, XBrushes.DarkSlateGray, 0, 30);
            gfx.DrawString("Times (bold)", fontBold, XBrushes.DarkSlateGray, 0, 65);
            gfx.DrawString("Times (italic)", fontItalic, XBrushes.DarkSlateGray, 0, 100);
            gfx.DrawString("Times (bold italic)", fontBoldItalic, XBrushes.DarkSlateGray, 0, 135);

            DrawHelper.EndBox(gfx);
        }

        void DrawTextAlignment(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "Text Alignment");
            XRect rect = new XRect(0, 0, 250, 140);

            XFont font = new XFont(DrawHelper.FontName, 10);
            XBrush brush = XBrushes.Purple;
            XStringFormat format = new XStringFormat();

            gfx.DrawRectangle(XPens.YellowGreen, rect);
            gfx.DrawLine(XPens.YellowGreen, rect.Width / 2, 0, rect.Width / 2, rect.Height);
            gfx.DrawLine(XPens.YellowGreen, 0, rect.Height / 2, rect.Width, rect.Height / 2);

            gfx.DrawString("TopLeft", font, brush, rect, format);

            format.Alignment = XStringAlignment.Center;
            gfx.DrawString("TopCenter", font, brush, rect, format);

            format.Alignment = XStringAlignment.Far;
            gfx.DrawString("TopRight", font, brush, rect, format);

            format.LineAlignment = XLineAlignment.Center;
            format.Alignment = XStringAlignment.Near;
            gfx.DrawString("CenterLeft", font, brush, rect, format);

            format.Alignment = XStringAlignment.Center;
            gfx.DrawString("Center", font, brush, rect, format);

            format.Alignment = XStringAlignment.Far;
            gfx.DrawString("CenterRight", font, brush, rect, format);

            format.LineAlignment = XLineAlignment.Far;
            format.Alignment = XStringAlignment.Near;
            gfx.DrawString("BottomLeft", font, brush, rect, format);

            format.Alignment = XStringAlignment.Center;
            gfx.DrawString("BottomCenter", font, brush, rect, format);

            format.Alignment = XStringAlignment.Far;
            gfx.DrawString("BottomRight", font, brush, rect, format);

            DrawHelper.EndBox(gfx);
        }

        void MeasureText(XGraphics gfx, int number)
        {
            DrawHelper.BeginBox(gfx, number, "Measure Text");

            const XFontStyle style = XFontStyle.Regular;
            XFont font = new XFont(DrawHelper.FontName, 95, style);

            const string text = "Hallo";
            const double x = 20, y = 100;
            XSize size = gfx.MeasureString(text, font);

            double lineSpace = font.GetHeight();
            int cellSpace = font.FontFamily.GetLineSpacing(style);
            int cellAscent = font.FontFamily.GetCellAscent(style);
            int cellDescent = font.FontFamily.GetCellDescent(style);
            int cellLeading = cellSpace - cellAscent - cellDescent;

            // Get effective ascent
            double ascent = lineSpace * cellAscent / cellSpace;
            gfx.DrawRectangle(XBrushes.Bisque, x, y - ascent, size.Width, ascent);

            // Get effective descent
            double descent = lineSpace * cellDescent / cellSpace;
            gfx.DrawRectangle(XBrushes.LightGreen, x, y, size.Width, descent);

            // Get effective leading
            double leading = lineSpace * cellLeading / cellSpace;
            gfx.DrawRectangle(XBrushes.Yellow, x, y + descent, size.Width, leading);

            // Draw text half transparent
            XColor color = XColors.DarkSlateBlue;
            color.A = 0.6;
            gfx.DrawString(text, font, new XSolidBrush(color), x, y);

            DrawHelper.EndBox(gfx);
        }
    }

    
}
