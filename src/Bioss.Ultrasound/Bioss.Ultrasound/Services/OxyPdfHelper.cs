using System;
using System.IO;
using System.Linq;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Extensions;
using Bioss.Ultrasound.UI.Helpers;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Bioss.Ultrasound.Services
{
    public class OxyPdfHelper
    {
        private readonly SizeHelper _horizontalSize = SizeHelper.HorizontalSize;
        private readonly SizeHelper _verticalSize = SizeHelper.VerticalSize;

        public XUnit ChartLength => _horizontalSize.ChartLength;
        public XUnit ChartHeight => _verticalSize.ChartLength;

        public PlotModel CreatePlotModel(Record record, PlottingHelper plottingHelper)
        {
            var chartDrawer = new ChartDrawer(plottingHelper)
            {
                IsPDF = true
            };

            var lightGridColor = OxyColor.FromRgb(180, 180, 180);

            var fhrAxis = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_FHR);
            fhrAxis.MajorStep = 30;
            fhrAxis.MinorStep = 10;
            fhrAxis.MinorGridlineStyle = LineStyle.Dot;
            fhrAxis.MinorGridlineThickness = .5;
            fhrAxis.MajorGridlineThickness = .5;
            fhrAxis.MinorGridlineColor = lightGridColor;
            fhrAxis.MajorGridlineColor = lightGridColor;
            fhrAxis.FontSize = 10;
            fhrAxis.Title = string.Empty;

            var fhrLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_FHR);
            fhrLine.StrokeThickness = 1;

            var tocoAxis = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_TOCO);
            tocoAxis.MajorStep = 20;
            tocoAxis.MinorStep = 10;
            tocoAxis.MinorGridlineStyle = LineStyle.Dot;
            tocoAxis.MinorGridlineThickness = .5;
            tocoAxis.MajorGridlineThickness = .5;
            tocoAxis.MinorGridlineColor = lightGridColor;
            tocoAxis.MajorGridlineColor = lightGridColor;
            tocoAxis.FontSize = 10;

            var tocoLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_TOCO);
            tocoLine.StrokeThickness = 1;

            var xAxis = chartDrawer.Model.Axes.First(a => a.Position == AxisPosition.Bottom);
            xAxis.MinorGridlineThickness = .5;
            xAxis.MajorGridlineThickness = 1;
            xAxis.MinorGridlineColor = lightGridColor;
            xAxis.MajorGridlineColor = OxyColors.Black;
            xAxis.FontSize = 10;
            xAxis.MajorStep = 3 * 60;
            xAxis.MinorStep = 60;
            xAxis.LabelFormatter = d => $"{d / 60}";

            chartDrawer.ResetFhrMinMax(30, 240);
            chartDrawer.Fill(record);

            return chartDrawer.Model;
        }

        public void PreparePage(PlotModel model, PlottingHelper plottingHelper, double pageStartMinutes)
        {
            plottingHelper.ResetAxisWithMin(TimeSpan.FromMinutes(pageStartMinutes));
            model.InvalidatePlot(true);
        }

        /// <summary>
        /// Рисует график как картинку в PDF.
        /// </summary>
        public void DrawChart(XGraphics graphics, PlotModel model)
        {
            var chartFileName = Path.GetTempFileName();

            try
            {
                SavePlotToPdfFile(chartFileName, model);

                using var image = XImage.FromFile(chartFileName);

                var top = XUnit.FromMillimeter(78).Point;
                var left = XUnit.FromMillimeter(5).Point;

                var width = image.PixelWidth * 72 / image.HorizontalResolution;
                var height = image.PixelHeight * 72 / image.HorizontalResolution;

                graphics.DrawImage(image, left, top, width, height);
            }
            finally
            {
                if (File.Exists(chartFileName))
                    File.Delete(chartFileName);
            }
        }

        /// <summary>
        /// Рисует квадраты времени поверх уже вставленного изображения графика.
        /// </summary>
        public void DrawTimeBoxes(
            XGraphics graphics,
            DateTime startTime,
            double pageStartMinutes,
            double minutesCountInPage)
        {
            const double majorStepMinutes = 3.0;

            // Эти координаты должны совпадать с позицией графика внутри PDF.
            var imageLeft = XUnit.FromMillimeter(5).Point;
            var imageTop = XUnit.FromMillimeter(78).Point;

            var chartLeft = imageLeft + XUnit.FromMillimeter(17).Point;
            var chartTop = imageTop + XUnit.FromMillimeter(1.5).Point;
            var chartWidth = XUnit.FromMillimeter(260).Point;

            var boxWidth = chartWidth * ((majorStepMinutes * 0.5) / minutesCountInPage);
            var boxHeight = XUnit.FromMillimeter(5).Point;

            var firstMajorMinute = Math.Ceiling(pageStartMinutes / majorStepMinutes) * majorStepMinutes;
            if (Math.Abs(firstMajorMinute - pageStartMinutes) < 0.001)
                firstMajorMinute += majorStepMinutes;

            var pageEndMinutes = pageStartMinutes + minutesCountInPage;

            var font = new XFont(
                PdfOrderConstants.FontName,
                PdfOrderConstants.FontSizeGrafic,
                XFontStyle.Bold);

            for (var minute = firstMajorMinute; minute < pageEndMinutes; minute += majorStepMinutes)
            {
                var offsetMinutes = minute - pageStartMinutes;
                var centerX = chartLeft + (offsetMinutes / minutesCountInPage) * chartWidth;

                var rect = new XRect(
                    centerX - boxWidth / 2,
                    chartTop,
                    boxWidth,
                    boxHeight);

                graphics.DrawRectangle(XPens.Black, XBrushes.White, rect);

                var timeText = startTime.AddMinutes(minute).ToString("HH:mm");
                graphics.DrawString(timeText, font, XBrushes.Black, rect, XStringFormats.Center);
            }
        }

        /// <summary>
        /// Рисует заголовки осей, так как OxyPlot не поддерживает Unicode в PDF как нужно.
        /// </summary>
        public void DrawChartTitles(XGraphics graphics, PdfPage page, int recordingSpeed)
        {
            var settings = new DrawStringSettings
            {
                Graphics = graphics,
                LineHeight = XUnit.FromMillimeter(2.5),
                PaddingLeft = XUnit.FromMillimeter(265),
                PaddingRight = XUnit.FromMillimeter(17),
                PaddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter + 73),
                Page = page,
            };

            settings.DrawString(
                PdfOrderConstants.FontSizeGrafic,
                0,
                XStringAlignment.Far,
                string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, recordingSpeed));

            var lineHeight = XUnit.FromMillimeter(2.5);
            var paddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter / 2 + 60);
            var paddingLeft = XUnit.FromMillimeter(9);

            var rotatePoint = new XPoint(paddingLeft, paddingTop);

            graphics.RotateAtTransform(-90, rotatePoint);
            graphics.DrawString(
                AppStrings.Chart_FHRAxysTitle,
                page,
                paddingTop,
                paddingLeft,
                PdfOrderConstants.FontSizeGrafic,
                lineHeight,
                0,
                XStringAlignment.Near);
            graphics.RotateAtTransform(90, rotatePoint);
        }

        private void SavePlotToPdfFile(string fileName, PlotModel model)
        {
            var tempFileName = Path.GetTempFileName();

            try
            {
                using (var stream = File.OpenWrite(tempFileName))
                {
                    var width = _horizontalSize.CanvasLength.Point;
                    var height = _verticalSize.CanvasLength.Point;

                    Export(model, stream, width, height, OxyColors.White);
                }

                // FIX oxyplot exporting to pdf
                var doc = PdfReader.Open(tempFileName);
                doc.Save(fileName);
            }
            finally
            {
                if (File.Exists(tempFileName))
                    File.Delete(tempFileName);
            }
        }

        private void Export(IPlotModel model, Stream stream, double width, double height, OxyColor background)
        {
            var exporter = new PdfExporter
            {
                Width = width,
                Height = height,
                Background = background
            };

            exporter.Export(model, stream);
        }

        private class SizeHelper
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