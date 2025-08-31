using System;
using System.IO;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.UI.Helpers;
using OxyPlot;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Bioss.Ultrasound.Tools.PdfTests
{
    // A4 210 × 297 mm

    public class PdfSharpAndOxyPlotPagesTest : IPdfTest
    {
        private readonly Record _record;

        public PdfSharpAndOxyPlotPagesTest(Record record)
        {
            _record = record;
        }

        public string Name => "PdfSharp + OxyPlot + Pages";

        public void CreatePdfFile(string fileName)
        {
            MyFontResolver.Apply();

            var document = new PdfDocument();
            document.Info.Title = "PDFsharp XGraphic Sample";
            document.Info.Author = "Stefan Lange";
            document.Info.Subject = "Created with code snippets that show the use of graphical functions";
            document.Info.Keywords = "PDFsharp, XGraphics";


            var count = 3;
            for(var i = 0; i < count; ++i)
            {
                var page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;

                XGraphics gfx = XGraphics.FromPdfPage(page);

                var img = new OxyImage(_record);
                img.DrawPage(gfx, document, page, count);

                DrawHelper.DrawHeader(gfx, page, "Moscow state hospital", DateTime.Now, "Ivanov Ivan, 29 years", "Brown");
                DrawHelper.DrawPageNumbers(gfx, page, i + 1, count);
            }

            document.Save(fileName);
        }

        class DrawHelper
        {
            public static string FontName = "Segoe UI";

            public static void DrawPageNumbers(XGraphics gfx, PdfPage page, int pageNumber, int pageCount)
            {
                var lineHeight = XUnit.FromMillimeter(2.5);
                var padding = XUnit.FromMillimeter(page.Height.Millimeter - 10);
                DrawString($"page {pageNumber} of {pageCount}", page, gfx, padding, 12, lineHeight, 0, XStringAlignment.Far);
            }

            public static void DrawHeader(XGraphics gfx, PdfPage page, string hospital, DateTime dateOfResearch, string patient, string doctor)
            {
                var padding = XUnit.FromMillimeter(5);
                var lineHeight = XUnit.FromMillimeter(4);

                DrawString($"{hospital}", page, gfx, padding, 12, lineHeight, 0, XStringAlignment.Center);

                DrawString("", page, gfx, padding, 12, lineHeight, 1, XStringAlignment.Center);

                DrawString("Date of research", page, gfx, padding, 10, lineHeight, 2, XStringAlignment.Near);
                DrawString($"{dateOfResearch}", page, gfx, padding, 12, lineHeight, 3, XStringAlignment.Near);

                DrawString("Patient", page, gfx, padding, 10, lineHeight, 2, XStringAlignment.Center);
                DrawString(patient, page, gfx, padding, 12, lineHeight, 3, XStringAlignment.Center);

                DrawString("Doctor", page, gfx, padding, 10, lineHeight, 2, XStringAlignment.Far);
                DrawString(doctor, page, gfx, padding, 12, lineHeight, 3, XStringAlignment.Far);
            }



            private static void DrawString(string text, PdfPage page, XGraphics gfx, XUnit paddingTop, int fontSize, XUnit lineHeight, int lineNumber, XStringAlignment textAlignment)
            {
                XFont font = new XFont(DrawHelper.FontName, fontSize);
                XBrush brush = XBrushes.Black;
                XStringFormat format = new XStringFormat();

                var lineHeightPoints = lineHeight.Point;
                var posY = lineNumber;
                var paddingTopPoints = paddingTop.Point;
                var paddingLeftPoints = XUnit.FromMillimeter(10).Point;
                var widthPoints = page.Width.Point - (paddingLeftPoints * 2);

                posY += (int)paddingTopPoints + (int)(lineHeightPoints * lineNumber);
                var heightPoints = lineHeightPoints;
                XRect rect = new XRect(paddingLeftPoints, posY, widthPoints, heightPoints);

                format.LineAlignment = XLineAlignment.Center;
                format.Alignment = textAlignment;
                gfx.DrawString(text, font, brush, rect, format);
            }
        }

        class OxyImage
        {
            private readonly Record _record;

            public OxyImage(Record record)
            {
                _record = record;
            }

            public void DrawPage(XGraphics gfx, PdfDocument document, PdfPage page, int count)
            {
                DrawFormXObject(gfx, page);
            }

            void DrawFormXObject(XGraphics gfx, PdfPage page)
            {
                var chartFileName = Path.Combine(Path.GetTempPath(), "OxyPdf.pdf");
                GenerateObjectFile(chartFileName, page);

                XImage image = XImage.FromFile(chartFileName);

                var top = XUnit.FromMillimeter(30).Point;
                var left = XUnit.FromMillimeter(10).Point;

                double width = image.PixelWidth * 72 / image.HorizontalResolution;
                double height = image.PixelHeight * 72 / image.HorizontalResolution;

                gfx.DrawImage(image, left, top, width, height);
            }

            private void GenerateObjectFile(string fileName, PdfPage page)
            {
                var model = GetPlot();
                model.DefaultFont = "Segoe UI";

                var fileName2 = Path.GetTempFileName();
                using (var stream = File.OpenWrite(fileName2))
                {
                    var topPadding = XUnit.FromMillimeter(30);
                    var bottomPadding = XUnit.FromMillimeter(10);
                    var leftPadding = XUnit.FromMillimeter(10);

                    var width = page.Width.Point - (leftPadding.Point * 2);
                    var height = page.Height.Point - topPadding.Point - bottomPadding.Point;

                    //exporter.Export(model, stream);
                    Export(model, stream, width, height, OxyColors.White);
                }

                // FIX oxyplot exporting to pdf
                var doc = PdfReader.Open(fileName2);
                doc.Save(fileName);
            }

            private PlotModel GetPlot()
            {
                var chartDrawer = new ChartDrawer(new PlottingHelper());

                chartDrawer.Fill(_record);

                return chartDrawer.Model;
            }

            public static void Export(IPlotModel model, Stream stream, double width, double height, OxyColor background)
            {
                var exporter = new PdfExporter { Width = width, Height = height, Background = background };
                exporter.Export(model, stream);
            }
        }
    }
}
