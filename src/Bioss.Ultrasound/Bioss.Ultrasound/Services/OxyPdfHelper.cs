using System;
using System.IO;
using System.Linq;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Extensions;
using Bioss.Ultrasound.UI.Helpers;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace Bioss.Ultrasound.Services
{
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
            fhrAxes.Title = string.Empty; //  удаляем заголовок, так к OxyPlot не сохраняет Unicode в PDF
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

            var xAxes = chartDrawer.Model.Axes.First(a => a.Position == AxisPosition.Bottom);
            xAxes.MinorGridlineThickness = .5;
            xAxes.MajorGridlineThickness = 1;
            xAxes.MinorGridlineColor = lightGridColor;
            xAxes.MajorGridlineColor = OxyColors.Black;
            xAxes.FontSize = 10;
            xAxes.MajorStep = 3 * 60;
            xAxes.MinorStep = 60;
            xAxes.LabelFormatter = (d) => $"{d / 60}";
            //xAxes.

            //
            chartDrawer.ResetFhrMinMax(30, 240);
            //
            chartDrawer.Fill(record);
            return chartDrawer.Model;
        }
       

        /// <summary>
        /// эта функция рисует заголовки для осей координат, так как OxyPlot не поддерживает Unicode
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="page"></param>
        /// <param name="recordingSpeed"></param>
        public void DrawChartTitles(XGraphics graphics, PdfPage page, int recordingSpeed)
        {
            var settings = new DrawStringSettings
            {
                Graphics = graphics,
                LineHeight = XUnit.FromMillimeter(2.5),
                PaddingLeft = XUnit.FromMillimeter(265),
                PaddingRight = XUnit.FromMillimeter(17),
                PaddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter + 83),
                Page = page,
            };

            // X - min
            settings.DrawString(PdfOrderConstants.FontSizeGrafic, 0, XStringAlignment.Far, string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, recordingSpeed));

            // Y - FHR
            var lineHeight = XUnit.FromMillimeter(2.5);
            var paddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter / 2 + 60);
            var paddingLeft = XUnit.FromMillimeter(9);

            var rotatePoint = new XPoint(paddingLeft, paddingTop);

            graphics.RotateAtTransform(-90, rotatePoint);
            graphics.DrawString(AppStrings.Chart_FHRAxysTitle, page, paddingTop, paddingLeft, PdfOrderConstants.FontSizeGrafic, lineHeight, 0, XStringAlignment.Near);
            graphics.RotateAtTransform(90, rotatePoint);
        }

        public void DrawChart(XGraphics gfx, PlotModel model, DateTime startTime)
        {
            AddTimeBoxAnnotationsToModel(model, startTime);
            var chartFileName = Path.GetTempFileName();

            SavePlotToPdfFile(chartFileName, model);

            XImage image = XImage.FromFile(chartFileName);

            var top = XUnit.FromMillimeter(88).Point;
            var left = XUnit.FromMillimeter(5).Point;

            double width = image.PixelWidth * 72 / image.HorizontalResolution;
            double height = image.PixelHeight * 72 / image.HorizontalResolution;

            gfx.DrawImage(image, left, top, width, height);
            //DrawTimeSquaresAboveGrid(gfx, model, top, width, height, startTime);
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

        private void AddTimeBoxAnnotationsToModel(PlotModel model, DateTime startTime)
        {
            var fhrAxis = model.Axes.First(a => a.Key == ChartDrawer.KEY_FHR);
            var xAxis = model.Axes.First(a => a.Position == AxisPosition.Bottom);
            if (xAxis == null || fhrAxis == null)
                return;

            // Параметры сетки (должны совпадать с настройками в GetPlotModel)
            var majorStepSeconds = xAxis.MajorStep;

            // ВАЖНО: Теперь ActualMinimum и ActualMaximum должны быть установлены
            double xMin = xAxis.ActualMinimum;
            double xMax = xAxis.ActualMaximum;

            // Размеры квадрата в единицах данных
            double boxWidthSeconds = majorStepSeconds * 0.5;
            double boxHeightValue = (fhrAxis.ActualMaximum - fhrAxis.ActualMinimum) * 0.025;

            // Позиция квадратов - выше графика
            double boxTopValue = fhrAxis.ActualMaximum * 1.12;

            var textPositionY = boxTopValue + boxHeightValue / 2;
            // Добавляем квадраты для каждой мажорной линии сетки
            // Пропускаем первую линию (0 минут) и добавляем для 3, 6, 9... минут
            for (double xSeconds = majorStepSeconds; xSeconds < xMax; xSeconds += majorStepSeconds)
            {
                // Пропускаем, если квадрат выйдет за границы графика
                if (xSeconds - boxWidthSeconds / 2 < xMin || xSeconds + boxWidthSeconds / 2 > xMax)
                    continue;

                //Добавляем текст времени
                var timeText = startTime.AddSeconds(xSeconds).ToString("HH:mm");
                var textAnnotation = new TextAnnotation
                {
                    Text = timeText,
                    TextPosition = new DataPoint(xSeconds, textPositionY),
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    TextColor = OxyColors.Black,
                    TextHorizontalAlignment = HorizontalAlignment.Center,
                    TextVerticalAlignment = VerticalAlignment.Middle,
                    Layer = AnnotationLayer.AboveSeries // Поверх квадрата
                };

                model.Annotations.Add(textAnnotation);
            }
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
