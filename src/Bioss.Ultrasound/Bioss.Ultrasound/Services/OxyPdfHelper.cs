using System;
using System.IO;
using System.Linq;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Tools;
using Bioss.Ultrasound.UI.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PdfSharpCore.Drawing;
using PdfSharpCore.Drawing.Layout;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Bioss.Ultrasound.Services
{
    public class Helper
    {
        private const string FontName = "Segoe UI";

        public void DrawHorizontalRullerTest(XGraphics gfx, XUnit leftMargin, XUnit len)
        {
            var marginTop = XUnit.FromMillimeter(44).Point;
            var marginLeft = leftMargin.Point;

            var pt1 = new XPoint(marginLeft, marginTop);
            var pt2 = new XPoint(marginLeft + len.Point, marginTop);

            gfx.DrawLine(new XPen(XBrushes.Black), pt1, pt2);
        }

        public void DrawVerticalRullerTest(XGraphics gfx, XUnit topMargin, XUnit len)
        {
            var marginTop = topMargin.Point;
            var marginLeft = XUnit.FromMillimeter(282).Point;

            var pt1 = new XPoint(marginLeft, marginTop);
            var pt2 = new XPoint(marginLeft, marginTop + len.Point);

            gfx.DrawLine(new XPen(XBrushes.Black), pt1, pt2);
        }

        public void DrawHeader(XGraphics graphics, PdfPage page, string hospital, DateTime dateOfResearch, string patient, string doctor)
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
            DrawString(settings, 12, 0, XStringAlignment.Near, AppStrings.PDF_HeaderLeftCorner);
            settings.PaddingLeft = tmpPaddingLeft;

            DrawString(settings, 12, 0, XStringAlignment.Center, $"{hospital}");
            DrawString(settings, 12, 1, XStringAlignment.Center, "");

            DrawString(settings, 9, 2, XStringAlignment.Near, AppStrings.PDF_HeaderDateOfResearch);
            DrawString(settings, 12, 3, XStringAlignment.Near, $"{dateOfResearch}");

            DrawString(settings, 9, 2, XStringAlignment.Center, AppStrings.PDF_HeaderPatient);
            DrawString(settings, 12, 3, XStringAlignment.Center, patient);

            DrawString(settings, 9, 2, XStringAlignment.Far, AppStrings.PDF_HeaderDoctor);
            DrawString(settings, 12, 3, XStringAlignment.Far, doctor);
        }

        public void DrawData(XGraphics gfx, PdfPage page, Biometric biometric, TimeSpan recordTime, int fetalsCount, DateTime dateOfResearch, DateTime? pregnancyStart)
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

            DrawString(settings, 9, 0, XStringAlignment.Near, $"{AppStrings.PDF_RecordingDuration}: {recordTime.Minutes} {AppStrings.Unit_min}");
            DrawString(settings, 9, 1, XStringAlignment.Near, $"{AppStrings.PDF_FetalMovements}: {fetalsCount}");
            if (pregnancyStart.HasValue)
            {
                var time = DateTools.CalculatePregnantTime(pregnancyStart.Value, dateOfResearch);
                DrawString(settings, 9, 2, XStringAlignment.Near, $"{AppStrings.PDF_GestationalAge}: {time.weeks}/{time.days}");

            }

            settings.PaddingLeft = XUnit.FromMillimeter(60);
            var empty = "-";
            DrawString(settings, 9, 0, XStringAlignment.Near, $"{AppStrings.PDF_Temperature}: {StringTools.ToStringOrEmptyString(biometric.Temperature, empty)}{AppStrings.Unit_Celsius}");
            DrawString(settings, 9, 1, XStringAlignment.Near, $"{AppStrings.PDF_Pulse}: {StringTools.ToStringOrEmptyString(biometric.Pulse, empty)} {AppStrings.Unit_HeartRate}");
            DrawString(settings, 9, 2, XStringAlignment.Near, $"{AppStrings.PDF_Sugar}: {StringTools.ToStringOrEmptyString(biometric.Sugar, empty)} {AppStrings.Unit_BloodGlucose}");
            DrawString(settings, 9, 3, XStringAlignment.Near, $"{AppStrings.PDF_HeartRate}: {StringTools.ToStringOrEmptyString(biometric.Systolic, empty)}/{StringTools.ToStringOrEmptyString(biometric.Diastolic, empty)} {AppStrings.Unit_MillimetreOfMercury}");

            settings.PaddingLeft = XUnit.FromMillimeter(60 + 45);
            settings.LineHeight = XUnit.FromMillimeter(4.5);

            DrawMultilineString(settings, 9, 0, 2, $"{AppStrings.PDF_Comment}: {biometric.Comment}");
            DrawMultilineString(settings, 9, 2, 2, $"{AppStrings.PDF_DoctorsConclusion}:");
        }

        public void DrawPageNumbers(XGraphics gfx, PdfPage page, int pageNumber, int pageCount)
        {
            var lineHeight = XUnit.FromMillimeter(2.5);
            var padding = XUnit.FromMillimeter(page.Height.Millimeter - 10);
            var paddingLeft = XUnit.FromMillimeter(10);
            DrawString(string.Format(AppStrings.PDF_FooterPageNumbers, pageNumber, pageCount), page, gfx, padding, paddingLeft, 10, lineHeight, 0, XStringAlignment.Far);
        }

        public void DrawDeviceSerialNumber(XGraphics gfx, PdfPage page, string serialNumber)
        {
            var lineHeight = XUnit.FromMillimeter(2.5);
            var padding = XUnit.FromMillimeter(page.Height.Millimeter - 10);
            var paddingLeft = XUnit.FromMillimeter(10);
            DrawString(string.Format(AppStrings.PDF_FooterSN, serialNumber), page, gfx, padding, paddingLeft, 10, lineHeight, 0, XStringAlignment.Near);
        }

        public static void DrawString(string text, PdfPage page, XGraphics gfx, XUnit paddingTop, XUnit paddingLeft, int fontSize, XUnit lineHeight, int lineNumber, XStringAlignment textAlignment)
        {
            var heightPoints = lineHeight.Point;
            var paddingTopPoints = paddingTop.Point;
            var paddingLeftPoints = paddingLeft.Point;
            var widthPoints = page.Width.Point - paddingLeftPoints * 2;

            var yPoints = paddingTopPoints + (heightPoints * lineNumber);
            var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

            var format = new XStringFormat
            {
                LineAlignment = XLineAlignment.Center,
                Alignment = textAlignment
            };
            var font = new XFont(FontName, fontSize);
            var brush = XBrushes.Black;
            gfx.DrawString(text, font, brush, rect, format);
        }

        public static void DrawString(DrawStringSettings settings, int fontSize, int lineNumber, XStringAlignment textAlignment, string text)
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
            var font = new XFont(FontName, fontSize);
            var brush = XBrushes.Black;
            settings.Graphics.DrawString(text, font, brush, rect, format);
        }

        public static void DrawRectangle(DrawStringSettings settings, XBrush brush)
        {
            var heightPoints = settings.LineHeight.Point;
            var paddingTopPoints = settings.PaddingTop.Point;
            var paddingLeftPoints = settings.PaddingLeft.Point;
            var paddingRightPoints = settings.PaddingRight.Point;
            var widthPoints = settings.Page.Width.Point - paddingLeftPoints - paddingRightPoints;

            var yPoints = paddingTopPoints;
            var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

            settings.Graphics.DrawRectangle(brush, rect);
        }

        private void DrawMultilineString(DrawStringSettings settings, int fontSize, int lineNumber, int lineCount, string text)
        {
            var lineHeight = settings.LineHeight.Point;
            var heightPoints = lineHeight * lineCount;
            var paddingTopPoints = settings.PaddingTop.Point;
            var paddingLeftPoints = settings.PaddingLeft.Point;
            var paddingRightPoints = settings.PaddingRight.Point;
            var widthPoints = settings.Page.Width.Point - paddingLeftPoints - paddingRightPoints;

            var yPoints = paddingTopPoints + (lineHeight * lineNumber);
            var rect = new XRect(paddingLeftPoints, yPoints, widthPoints, heightPoints);

            var font = new XFont(FontName, fontSize);
            var brush = XBrushes.Black;

            var formatter = new XTextFormatter(settings.Graphics);
            formatter.DrawString(text, font, brush, rect, XStringFormats.TopLeft);
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

    public class OxyPdfHelper
    {
        private SizeHelper _horizontalSize = SizeHelper.HorizontalSize;
        private SizeHelper _verticalSize = SizeHelper.VerticalSize;

        public XUnit ChartLength => _horizontalSize.ChartLength;
        public XUnit ChartHeight => _verticalSize.ChartLength;

        public PlotModel GetPlotModel(Record record, PlottingHelper plottingHelper)
        {
            var chartDrawer = new ChartDrawer(plottingHelper);

            // Перенастраеваем график для того чтоб он принял стиль нужный в PDF
            chartDrawer.IsPDF = true;

            var lightGridColor = OxyColor.FromRgb(180, 180, 180);

            var fhrAxes = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_FHR);
            fhrAxes.MajorStep = 30;
            fhrAxes.MinorStep = 10;
            fhrAxes.MinorGridlineStyle = LineStyle.Dot;
            fhrAxes.MinorGridlineThickness = .5;
            fhrAxes.MajorGridlineThickness = .5;
            fhrAxes.MinorGridlineColor = lightGridColor;
            fhrAxes.MajorGridlineColor = lightGridColor;
            fhrAxes.FontSize = 10;
            fhrAxes.Title = ""; //  удаляем заголовок, так к OxyPlot не сохраняет Unicode в PDF
            var fhrLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_FHR);
            fhrLine.StrokeThickness = 1;

            var tocoAxes = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_TOCO);
            tocoAxes.MajorStep = 20;
            tocoAxes.MinorStep = 10;
            tocoAxes.MinorGridlineStyle = LineStyle.Dot;
            tocoAxes.MinorGridlineThickness = .5;
            tocoAxes.MajorGridlineThickness = .5;
            tocoAxes.MinorGridlineColor = lightGridColor;
            tocoAxes.MajorGridlineColor = lightGridColor;
            tocoAxes.FontSize = 10;
            var tocoLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_TOCO);
            tocoLine.StrokeThickness = 1;

            var xAxes = chartDrawer.Model.Axes.First(a => a.Position == AxisPosition.Bottom); ;
            xAxes.MinorGridlineThickness = .5;
            xAxes.MajorGridlineThickness = 1;
            xAxes.MinorGridlineColor = lightGridColor;
            xAxes.MajorGridlineColor = OxyColors.Black;
            xAxes.FontSize = 10;
            xAxes.MajorStep = 3 * 60;
            xAxes.MinorStep = 60;
            xAxes.LabelFormatter = (d) => $"{d / 60}";

            //
            chartDrawer.ResetFhrMinMax(30, 240);
            //
            chartDrawer.Fill(record);
            return chartDrawer.Model;
        }

        public void DrawChart(XGraphics gfx, PlotModel model)
        {
            var chartFileName = Path.GetTempFileName();

            SavePlotToPdfFile(chartFileName, model);

            XImage image = XImage.FromFile(chartFileName);

            var top = XUnit.FromMillimeter(80).Point;
            var left = XUnit.FromMillimeter(5).Point;

            double width = image.PixelWidth * 72 / image.HorizontalResolution;
            double height = image.PixelHeight * 72 / image.HorizontalResolution;

            gfx.DrawImage(image, left, top, width, height);
        }

        //  эта функция рисует заголовки для осей координат, так как OxyPlot не поддерживает Unicode
        public void DrawChartTitles(XGraphics graphics, PdfPage page, int recordingSpeed)
        {
            var settings = new Helper.DrawStringSettings
            {
                Graphics = graphics,
                LineHeight = XUnit.FromMillimeter(2.5),
                PaddingLeft = XUnit.FromMillimeter(265),
                PaddingRight = XUnit.FromMillimeter(17),
                PaddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter + 68),
                Page = page,
            };

            // X - min
            Helper.DrawString(settings, 8, 0, XStringAlignment.Far, string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, recordingSpeed));

            // Y - FHR
            var lineHeight = XUnit.FromMillimeter(2.5);
            var paddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter / 2 + 60);
            var paddingLeft = XUnit.FromMillimeter(9);

            var rotatePoint = new XPoint(paddingLeft, paddingTop);

            graphics.RotateAtTransform(-90, rotatePoint);
            Helper.DrawString(AppStrings.Chart_FHRAxysTitle, page, graphics, paddingTop, paddingLeft, 8, lineHeight, 0, XStringAlignment.Near);
            graphics.RotateAtTransform(90, rotatePoint);
        }

        //  todo: нужно передавать размеры
        private void SavePlotToPdfFile(string fileName, PlotModel model)
        {
            var fileName2 = Path.GetTempFileName();
            using (var stream = File.OpenWrite(fileName2))
            {
                var width = _horizontalSize.CanvasLength.Point;
                var height = _verticalSize.CanvasLength.Point;

                var chartBackground = OxyColors.White;
                Export(model, stream, width, height, chartBackground);
            }

            // FIX oxyplot exporting to pdf
            var doc = PdfReader.Open(fileName2);
            doc.Save(fileName);
        }

        private void Export(IPlotModel model, Stream stream, double width, double height, OxyColor background)
        {
            var exporter = new PdfExporter { Width = width, Height = height, Background = background };
            exporter.Export(model, stream);
        }

        class SizeHelper
        {
            private readonly XUnit _axisLength = XUnit.FromMillimeter(17);
            private readonly XUnit _borderLength = XUnit.FromMillimeter(2.5);

            public SizeHelper(XUnit axisLength, XUnit borderLength, XUnit chartLength)
            {
                _axisLength = axisLength;
                _borderLength = borderLength;
                ChartLength = chartLength;
            }

            public XUnit ChartLength { get; }

            public XUnit CanvasLength => XUnit.FromPoint(_axisLength.Point + ChartLength.Point + _borderLength.Point);

            public static SizeHelper HorizontalSize => new(
                        axisLength: XUnit.FromMillimeter(17),
                        borderLength: XUnit.FromMillimeter(2.5),
                        chartLength: XUnit.FromMillimeter(260));

            public static SizeHelper VerticalSize => new(
                        axisLength: XUnit.FromMillimeter(10.5),
                        borderLength: XUnit.FromMillimeter(2.5),
                        chartLength: XUnit.FromMillimeter(102));
        }
    }
}
