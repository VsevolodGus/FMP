//using Bioss.Ultrasound.Core.Domain.Models;
//using Bioss.Ultrasound.Core.Services;
//using Bioss.Ultrasound.Core.Services.Constants;
//using Bioss.Ultrasound.Domain.Plotting;
//using Bioss.Ultrasound.Resources.Localization;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Pdf;
//using PdfSharpCore.Pdf.IO;

//namespace Bioss.Ultrasound.Core.Services;

//public class OxyPdfHelper
//{
//    private SizeHelper _horizontalSize = SizeHelper.HorizontalSize;
//    private SizeHelper _verticalSize = SizeHelper.VerticalSize;

//    public XUnit ChartLength => _horizontalSize.ChartLength;
//    public XUnit ChartHeight => _verticalSize.ChartLength;

//    /// <summary>
//    /// Создание графика на одну страницу
//    /// </summary>
//    /// <param name="record">запись по которой будет строиться график</param>
//    /// <param name="plottingHelper"></param>
//    /// <param name="pageTimeMinutes">кол-во минут на странице</param>
//    /// <param name="pageNumber">номер страницы, чтобы пагинировать графики</param>
//    /// <returns>модельь графика на 1 страницу</returns>
//    public PlotModel GetPlotModel(Record record, PlottingHelper plottingHelper, double pageTimeMinutes, int pageNumber)
//    {
//        var chartDrawer = new ChartDrawer(plottingHelper);

//        // Перенастраеваем график для того чтоб он принял стиль нужный в PDF
//        chartDrawer.IsPDF = true;

//        var lightGridColor = OxyColor.FromRgb(180, 180, 180);

//        var fhrAxes = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_FHR);
//        fhrAxes.MajorStep = 30;
//        fhrAxes.MinorStep = 10;
//        fhrAxes.MinorGridlineStyle = LineStyle.Dot;
//        fhrAxes.MinorGridlineThickness = .5;
//        fhrAxes.MajorGridlineThickness = .5;
//        fhrAxes.MinorGridlineColor = lightGridColor;
//        fhrAxes.MajorGridlineColor = lightGridColor;
//        fhrAxes.FontSize = 10;
//        fhrAxes.Title = string.Empty; //  удаляем заголовок, так к OxyPlot не сохраняет Unicode в PDF
//        var fhrLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_FHR);
//        fhrLine.StrokeThickness = 1;

//        var tocoAxes = chartDrawer.Model.Axes.First(a => a.Key == ChartDrawer.KEY_TOCO);
//        tocoAxes.MajorStep = 20;
//        tocoAxes.MinorStep = 10;
//        tocoAxes.MinorGridlineStyle = LineStyle.Dot;
//        tocoAxes.MinorGridlineThickness = .5;
//        tocoAxes.MajorGridlineThickness = .5;
//        tocoAxes.MinorGridlineColor = lightGridColor;
//        tocoAxes.MajorGridlineColor = lightGridColor;
//        tocoAxes.FontSize = 10;
//        var tocoLine = (LineSeries)chartDrawer.Model.Series.First(a => ((LineSeries)a).YAxisKey == ChartDrawer.KEY_TOCO);
//        tocoLine.StrokeThickness = 1;

//        var xAxes = chartDrawer.Model.Axes.First(a => a.Position == AxisPosition.Bottom);
//        xAxes.MinorGridlineThickness = .5;
//        xAxes.MajorGridlineThickness = 1;
//        xAxes.MinorGridlineColor = lightGridColor;
//        xAxes.MajorGridlineColor = OxyColors.Black;
//        xAxes.FontSize = 10;
//        xAxes.MajorStep = 3 * 60;
//        xAxes.MinorStep = 60;
//        xAxes.LabelFormatter = (d) => $"{d / 60}";

//        //
//        chartDrawer.ResetFhrMinMax(30, 240);
//        //
//        chartDrawer.Fill(record);
//        plottingHelper.ResetAxisWithMin(TimeSpan.FromMinutes(pageTimeMinutes));
//        foreach (var annotaion in AddTimeBoxAnnotationsToModel(fhrAxes, xAxes, record.StartTime, pageNumber))
//            chartDrawer.Model.Annotations.Add(annotaion);

//        return chartDrawer.Model;
//    }

//    /// <summary>
//    /// Добавление квадратов с времени на мажорных линиях
//    /// Вызывать после вызова plottingHelper.ResetAxisWithMin
//    /// </summary>
//    /// <param name="fhrAxis">используется для рассчета размера коробки </param>
//    /// <param name="xAxis">используется для рассчета расположения квадратов</param>
//    /// <param name="startTime">время начала записи</param>
//    /// <param name="pageNumber">номер страницы</param>
//    /// <returns>анотации графика</returns>
//    private IEnumerable<TextAnnotation> AddTimeBoxAnnotationsToModel(Axis fhrAxis, Axis xAxis, DateTime startTime, int pageNumber)
//    {
//        // Параметры сетки (должны совпадать с настройками в GetPlotModel)
//        var majorStepSeconds = xAxis.MajorStep;
//        var minorStepSeconds = xAxis.MinorStep;

//        double xMin = xAxis.ActualMinimum;
//        double xMax = xAxis.ActualMaximum;

//        // Размеры квадрата в единицах данных
//        double boxWidthSeconds = majorStepSeconds * 0.5;
//        double boxHeightValue = (fhrAxis.ActualMaximum - fhrAxis.ActualMinimum) * 0.025;

//        // Позиция квадратов - выше графика
//        double boxTopValue = fhrAxis.ActualMaximum * 1.12;

//        var textPositionY = boxTopValue + boxHeightValue / 2;
//        // Добавляем квадраты для каждой мажорной линии сетки
//        // Пропускаем первую линию (0 минут) и добавляем для 3, 6, 9... минут
//        for (double xSeconds = 0; xSeconds < xMax - boxWidthSeconds; xSeconds += majorStepSeconds)
//        {
//            // Пропускаем, если квадрат выйдет за границы графика
//            var seconds = xSeconds + (xMax + minorStepSeconds) * pageNumber;
//            if (seconds % xMax - boxWidthSeconds / 2 < xMin || seconds % xMax + boxWidthSeconds / 2 > xMax)
//                continue;

//            //Добавляем текст времени
//            var timeText = startTime.AddSeconds(seconds).ToString("HH:mm");
//            yield return new TextAnnotation
//            {
//                Text = timeText,
//                TextPosition = new DataPoint(seconds, textPositionY),
//                FontSize = PdfOrderConstants.FontSizeGrafic,
//                FontWeight = FontWeights.Bold,
//                TextColor = OxyColors.Black,
//                TextHorizontalAlignment = HorizontalAlignment.Center,
//                TextVerticalAlignment = VerticalAlignment.Middle,
//                Layer = AnnotationLayer.AboveSeries // Поверх квадрата
//            };
//        }
//    }


//    /// <summary>
//    /// эта функция рисует заголовки для осей координат, так как OxyPlot не поддерживает Unicode
//    /// </summary>
//    /// <param name="graphics">данные графики ПДФ файла</param>
//    /// <param name="page">страница пдф</param>
//    /// <param name="recordingSpeed">масштаб графика</param>
//    public void DrawChartTitles(XGraphics graphics, PdfPage page, int recordingSpeed)
//    {
//        var settings = new DrawStringSettings
//        {
//            Graphics = graphics,
//            LineHeight = XUnit.FromMillimeter(2.5),
//            PaddingLeft = XUnit.FromMillimeter(265),
//            PaddingRight = XUnit.FromMillimeter(17),
//            PaddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter + 73),
//            Page = page,
//        };

//        // X - min
//        settings.DrawString(PdfOrderConstants.FontSizeGrafic, 0, XStringAlignment.Far, string.Format(AppStrings.Settings_PdfRecordingSpeedFormat, recordingSpeed));

//        // Y - FHR
//        var lineHeight = XUnit.FromMillimeter(2.5);
//        var paddingTop = XUnit.FromMillimeter(_verticalSize.CanvasLength.Millimeter / 2 + 60);
//        var paddingLeft = XUnit.FromMillimeter(9);

//        var rotatePoint = new XPoint(paddingLeft, paddingTop);

//        graphics.RotateAtTransform(-90, rotatePoint);
//        graphics.DrawString(AppStrings.Chart_FHRAxysTitle, page, paddingTop, paddingLeft, PdfOrderConstants.FontSizeGrafic, lineHeight, 0, XStringAlignment.Near);
//        graphics.RotateAtTransform(90, rotatePoint);
//    }

//    /// <summary>
//    /// Функция отрисовки графика как картинки в ПФД отчете
//    /// </summary>
//    /// <param name="gfx">настройки ПДФ файла</param>
//    /// <param name="model">модель графика</param>
//    public void DrawChart(XGraphics gfx, PlotModel model)
//    {
//        var chartFileName = Path.GetTempFileName();

//        SavePlotToPdfFile(chartFileName, model);

//        XImage image = XImage.FromFile(chartFileName);

//        var top = XUnit.FromMillimeter(78).Point;
//        var left = XUnit.FromMillimeter(5).Point;

//        double width = image.PixelWidth * 72 / image.HorizontalResolution;
//        double height = image.PixelHeight * 72 / image.HorizontalResolution;

//        gfx.DrawImage(image, left, top, width, height);
//    }
//    //  todo: нужно передавать размеры
//    private void SavePlotToPdfFile(string fileName, PlotModel model)
//    {
//        var fileName2 = Path.GetTempFileName();
//        using (var stream = File.OpenWrite(fileName2))
//        {
//            var width = _horizontalSize.CanvasLength.Point;
//            var height = _verticalSize.CanvasLength.Point;

//            var chartBackground = OxyColors.White;
//            Export(model, stream, width, height, chartBackground);
//        }

//        // FIX oxyplot exporting to pdf
//        var doc = PdfReader.Open(fileName2);
//        doc.Save(fileName);
//    }

//    private void Export(IPlotModel model, Stream stream, double width, double height, OxyColor background)
//    {
//        var exporter = new PdfExporter { Width = width, Height = height, Background = background };
//        exporter.Export(model, stream);
//    }

//    class SizeHelper
//    {
//        private readonly XUnit _axisLength = XUnit.FromMillimeter(17);
//        private readonly XUnit _borderLength = XUnit.FromMillimeter(2.5);

//        public SizeHelper(XUnit axisLength, XUnit borderLength, XUnit chartLength)
//        {
//            _axisLength = axisLength;
//            _borderLength = borderLength;
//            ChartLength = chartLength;
//        }

//        public XUnit ChartLength { get; }

//        public XUnit CanvasLength => XUnit.FromPoint(_axisLength.Point + ChartLength.Point + _borderLength.Point);

//        public static SizeHelper HorizontalSize => new(
//                    axisLength: XUnit.FromMillimeter(17),
//                    borderLength: XUnit.FromMillimeter(2.5),
//                    chartLength: XUnit.FromMillimeter(260));

//        public static SizeHelper VerticalSize => new(
//                    axisLength: XUnit.FromMillimeter(10.5),
//                    borderLength: XUnit.FromMillimeter(2.5),
//                    chartLength: XUnit.FromMillimeter(102));
//    }
//}
