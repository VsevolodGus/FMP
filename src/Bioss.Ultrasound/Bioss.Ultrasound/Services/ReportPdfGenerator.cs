using System;
using Bioss.Ultrasound.Domain.Constants;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Abstracts;
using Bioss.Ultrasound.Services.Extensions;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Tools;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Table = MigraDocCore.DocumentObjectModel.Tables.Table;

namespace Bioss.Ultrasound.Services
{
    public class ReportPdfGenerator : IPdfGenerator
    {
        private const string Style = "MyTableStyle";
        private const string FloatFormat = "0.0";

        private readonly ILogger _logger;
        private readonly CatAnaService _catAnaService;
        private readonly InfoSettingsService _infoService;

        public ReportPdfGenerator(
            ILogger logger,
            CatAnaService catAnaService,
            InfoSettingsService infoService)
        {
            MyFontResolver.Apply();

            _logger = logger;
            _infoService = infoService;
            _catAnaService = catAnaService;
        }

        public void GenerateToFile(string fileName, Record record)
        {
            var document = CreateDocument();
            AddData(document, record);
            document.Save(fileName);
            _logger.Log($"Convert record to PDF: {fileName}");
        }

        private PdfDocument CreateDocument()
        {
            var document = new PdfDocument();
            document.Info.Title = "Ultrasound";
            document.Info.Author = "Medical Computer Systems";
            document.Info.Subject = "Created with Ultrasound app";
            document.Info.Keywords = "Ultrasound, MCS, Pregnant";
            return document;
        }

        private void AddData(PdfDocument document, Record record)
        {
            var hospital = SettingsValueToPdf(_infoService.Organization);
            var doctor = SettingsValueToPdf(_infoService.Doctor);

            var age = _infoService.Birthday.HasValue
                ? $", {string.Format(AppStrings.PDF_UserBirthdayDescriptionAge, _infoService.Birthday.Value.CalculateAge())}"
                : string.Empty;

            var patient = _infoService.IsPersonalDevice
                ? $"{_infoService.Patient ?? "-"}{age}"
                : AppStrings.PDF_NotSpecified;

            var oxyHelper = new OxyPdfHelper();
            var minutesCountInPage = ReportExtensions.CalculateMinuteInPage(
                oxyHelper.ChartLength.Centimeter,
                _infoService.PdfRecordingSpeed);

            var pages = ReportExtensions.CalculateCountPages(
                (int)record.RecordingTimeSpan.TotalMinutes,
                (int)minutesCountInPage);

            var plottingHelper = new PlottingHelper
            {
                Scale = minutesCountInPage * Constants.CountMinuteInHours
            };

            var cardiografy = record.CardiotocographyInfo ?? _catAnaService.CargiographAnalayzeWithUserSettings(record);
            var comment = cardiografy.IsRoodDawsonCriteriaValid()
                ? AppStrings.PDF_DawsonRedmanCriteriaMet
                : string.Format(AppStrings.PDF_DawsonRedmanCriteriaNoMet, cardiografy.CountRoodDawsonCriteriaValid());

            var (tableDocument, table) = DrawDataInTable(record, cardiografy);
            var tablePosition = new XPoint(XUnit.FromMillimeter(10), XUnit.FromMillimeter(25));

            var docRenderer = new DocumentRenderer(tableDocument);
            docRenderer.PrepareDocument();

            var model = oxyHelper.CreatePlotModel(record, plottingHelper);

            for (var i = 0; i < pages; ++i)
            {
                var page = document.AddPage();
                page.Orientation = PageOrientation.Landscape;

                using var graphics = XGraphics.FromPdfPage(page);
                graphics.MUH = PdfFontEncoding.Unicode;

                var pageStartMinutes = i * minutesCountInPage;

                oxyHelper.PreparePage(model, plottingHelper, pageStartMinutes);

                oxyHelper.DrawChart(graphics, model);
                oxyHelper.DrawTimeBoxes(graphics, record.StartTime, pageStartMinutes, minutesCountInPage);
                oxyHelper.DrawChartTitles(graphics, page, _infoService.PdfRecordingSpeed);

                graphics.DrawHeader(page, hospital, record.StartTime, patient, doctor, _infoService.PregnancyWeek, _infoService.PregnancyDay);
                graphics.DrawPageNumbers(page, i + 1, pages);
                graphics.DrawDeviceSerialNumber(page, record.DeviceSerialNumber ?? string.Empty);

                docRenderer.RenderObject(graphics, tablePosition.X, tablePosition.Y, PdfOrderConstants.WidthA4, table);

                graphics.DrawString(comment, page, 205, 30, PdfOrderConstants.HeaderFontSize, 0, 1, XStringAlignment.Near, XFontStyle.Bold);
            }
        }

        #region Построение таблицы

        public (Document, Table) DrawDataInTable(Record record, CardiotocographyInfo cardiografy)
        {
            var document = new Document();
            var section = document.AddSection();
            var table = section.AddTable();

            #region Общие настройки таблицы
            var tableStyle = document.Styles.AddStyle(Style, StyleNames.Normal);
            tableStyle.Font.Size = PdfOrderConstants.DefaultFontSize;
            table.Format.Font.Name = PdfOrderConstants.FontName;
            table.Borders.Color = Colors.Black;

            table.Style = Style;
            table.Borders.Visible = true;
            table.Borders.Width = 0.75;
            table.Rows.LeftIndent = 5;
            #endregion

            #region Создаем колонки
            table.AddColumn("0.5cm");
            table.AddColumn("5cm");
            table.AddColumn("1.1cm");
            table.AddColumn("0.5cm");
            table.AddColumn("5cm");
            table.AddColumn("1.7cm");
            table.AddColumn("4cm");
            table.AddColumn("1.4cm");
            table.AddColumn("8.5cm");
            #endregion

            #region Создаем строки
            var row0 = BuildRow0(table, cardiografy);
            BuildRow1(table, cardiografy, record.Biometric);
            BuildRow2(table, cardiografy, record.Biometric);
            BuildRow3(table, cardiografy, record.Biometric);
            BuildRow4(table, cardiografy, record.Biometric);
            BuildRow5(table, cardiografy);
            var row6 = BuildRow6(table, cardiografy);
            BuildRow7(table, cardiografy);
            #endregion

            #region Комментарии пациента
            var cell66 = row6[6];
            var commentPatient = $"{AppStrings.PDF_Comment}: {record.Biometric?.Comment ?? string.Empty}";
            cell66.AddParagraph(commentPatient);
            cell66.Format.Font.Bold = true;
            cell66.MergeRight = 2;
            cell66.MergeDown = 1;
            #endregion

            #region Текст под *
            var cell63 = row6[3];
            if (_infoService.PregnancyWeek >= CardiograhyConstants.BoundaryWeekOfTimeDependentParameters)
                cell63.AddParagraph(AppStrings.PDF_EpisodeCondition28Plus);
            else
                cell63.AddParagraph(AppStrings.PDF_EpisodeConditionSTV);

            cell63.Format.Font.Italic = true;
            cell63.MergeRight = 2;
            cell63.MergeDown = 1;
            #endregion

            #region Заключение
            var cell08 = row0.Cells[8];
            cell08.AddParagraph(AppStrings.PDF_DoctorsConclusion);
            cell08.Format.Font.Bold = true;
            cell08.MergeDown = 4;
            #endregion

            return (document, table);
        }

        #region Отрисовка строк

        private Row BuildRow0(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[0].FillBoolCell(cardiotocography.IsValidRecordingDuration);
            row.Cells[1].FillCell(AppStrings.PDF_RecordingDurationMin10);
            row.Cells[2].FillCell(cardiotocography.RecordingDuration?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[3].FillBoolCell(cardiotocography.IsValidSyncRhythm);
            row.Cells[4].FillCell(AppStrings.PDF_SinusRhythmMin2);
            row.Cells[5].FillCell(cardiotocography.SyncRhythmMinutes.ToString());

            var cell6 = row.Cells[6];
            cell6.AddParagraph(AppStrings.PDF_MotherParameters);
            cell6.Format.Font.Bold = true;
            cell6.Format.Alignment = ParagraphAlignment.Left;

            row.Cells[7].FillCell();

            return row;
        }

        private Row BuildRow1(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[0].FillBoolCell(cardiotocography.SignalLossValid);
            row.Cells[1].FillCell(AppStrings.PDF_SignalLossMax20);
            row.Cells[2].FillCell(cardiotocography.SignalLossPercentage?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[3].FillCell();
            row.Cells[4].FillCell(AppStrings.PDF_LTV);

            var textCell5 = cardiotocography.BeatLTV.HasValue && cardiotocography.TimeMsLTV.HasValue
                ? $"{cardiotocography.TimeMsLTV.Value.ToString(FloatFormat)}({cardiotocography.BeatLTV.Value.ToString(FloatFormat)})"
                : PdfOrderConstants.DefaultDoubleValue;

            row.Cells[5].FillCell(textCell5);

            row.Cells[6].FillCell(AppStrings.PDF_TemperatureC);
            row.Cells[7].FillCell(biometric?.Temperature.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            return row;
        }

        private Row BuildRow2(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[0].FillBoolCell(cardiotocography.BasalHeartRateValid);
            row.Cells[1].FillCell(AppStrings.PDF_BasalFHRRange110_160);
            row.Cells[2].FillCell(cardiotocography.BasalHeartRate?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[3].FillBoolCell(cardiotocography.STVValid);
            row.Cells[4].FillCell(AppStrings.PDF_STVMin4);
            row.Cells[5].FillCell(cardiotocography.STV?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[6].FillCell(AppStrings.PDF_PulseBpm);
            row.Cells[7].FillCell(biometric?.Pulse.ToString() ?? PdfOrderConstants.DefaultValue);

            return row;
        }

        private Row BuildRow3(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[1].FillCell(AppStrings.PDF_AccCountMore10);
            row.Cells[2].FillCell(cardiotocography.AccelerationsOver10?.ToString() ?? PdfOrderConstants.DefaultValue);
            row.Cells[3].FillCell();

            row.Cells[4].FillCell(AppStrings.PDF_OscillationFrequency);
            row.Cells[5].FillCell(cardiotocography.OscillationFrequency?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[6].FillCell(AppStrings.PDF_SugarMmolL);
            row.Cells[7].FillCell(biometric?.Sugar.ToString() ?? PdfOrderConstants.DefaultValue);

            return row;
        }

        private Row BuildRow4(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[1].FillCell(AppStrings.PDF_AccCountMore15);
            row.Cells[2].FillCell(cardiotocography.AccelerationsOver15?.ToString() ?? PdfOrderConstants.DefaultValue);

            row.Cells[3].FillBoolCell(cardiotocography.MovementFrequencyValid);

            row.Cells[4].FillCell(AppStrings.PDF_UCFrequencyMin3);
            row.Cells[5].FillCell(cardiotocography.MovementFrequency?.ToString(FloatFormat) ?? PdfOrderConstants.DefaultValue);

            row.Cells[6].FillCell(AppStrings.PDF_HeartRate);
            var systolic = biometric?.Systolic.ToString() ?? PdfOrderConstants.DefaultValue;
            var diastolic = biometric?.Diastolic.ToString() ?? PdfOrderConstants.DefaultValue;
            row.Cells[7].FillCell($"{systolic}/{diastolic}");

            return row;
        }

        private Row BuildRow5(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[0].FillBoolCell(cardiotocography.DecelerationsMark);
            row.Cells[1].FillCell(AppStrings.PDF_DecCountMore20);
            row.Cells[2].FillCell(cardiotocography.Decelerations?.ToString() ?? PdfOrderConstants.DefaultValue);

            row.Cells[3].FillBoolCell(cardiotocography.IsTimeDependentParameters);
            row.Cells[4].FillCell(AppStrings.PDF_TimeDependentParameters);
            row.Cells[5].FillCell($"{_infoService.PregnancyWeek}/{_infoService.PregnancyDay}");

            var cell6 = row.Cells[6];
            cell6.MergeRight = 2;

            return row;
        }

        private Row BuildRow6(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[1].FillCell(AppStrings.PDF_HighVariability);
            row.Cells[2].FillCell(cardiotocography.HighVariabilityMinutes?.ToString() ?? PdfOrderConstants.DefaultValue);

            return row;
        }

        private Row BuildRow7(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeTableRow;

            row.Cells[1].FillCell(AppStrings.PDF_LowVariability);
            row.Cells[2].FillCell(cardiotocography.LowVariabilityMinutes?.ToString() ?? PdfOrderConstants.DefaultValue);

            return row;
        }

        #endregion
        #endregion

        private string SettingsValueToPdf(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? AppStrings.PDF_NotSpecified
                : value;
        }
    }
}