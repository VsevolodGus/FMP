using System;
using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Abstracts;
using Bioss.Ultrasound.Services.Extensions;
using Bioss.Ultrasound.Tools;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Table = MigraDocCore.DocumentObjectModel.Tables.Table;

namespace Bioss.Ultrasound.Services
{
    public class ReportPdfGenerator : IPdfGenerator
    {
        private const string Style = "MyTableStyle2";
        private const string FloatFormat = "0.00";

        private readonly CatAnaService _catAnaService;
        private readonly InfoSettingsService _infoService;
        
        public ReportPdfGenerator(
            CatAnaService catAnaService,
            InfoSettingsService infoService
            )
        {
            _infoService = infoService;
            _catAnaService = catAnaService;
        }

        // A4 210 × 297 mm
        public void GenerateToFile(string fileName, Record record)
        {
            MyFontResolver.Apply();

            var document = CreateDocument();
            AddData(document, record);
            document.Save(fileName);
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

            var researchTime = record.StartTime;
            var serialNumber = record.DeviceSerialNumber ?? string.Empty;
            var fetalsCount = record.Events.Count(a => a.Event == Events.FetalMovement);

            //  сколько секунд отображать на одной странице
            var oxyHelper = new OxyPdfHelper();
            var minutesCountInPage = ReportExtensions.CalculateMinuteInPage(oxyHelper.ChartLength.Centimeter, _infoService.PdfRecordingSpeed);
            var pages = ReportExtensions.CalculateCountPages((int)record.RecordingTimeSpan.TotalMinutes, (int)minutesCountInPage);


            var plottingHelper = new PlottingHelper();
            plottingHelper.Scale = minutesCountInPage * Constants.CountMinuteInHours;
            var pregnancyDate = _infoService.PregnancyStart ?? DateTools.GetDefaultPregnancyDate();

            var floatRate = record.Fhrs.Select(c => (float)c.Fhr).ToArray();
            var movement = record.Events.Select(c => c.Event == Events.FetalMovement).ToArray();
            var cardiografy = _catAnaService.CargiographAnalayzeWithUserSettings(pregnancyDate.CalculatePregnantTime().weeks, floatRate, movement);
            for (var i = 0; i < pages; ++i)
            {
                var page = document.AddPage();
                page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                var graphics = XGraphics.FromPdfPage(page);

                var model = oxyHelper.GetPlotModel(record, plottingHelper);
                var time = i * minutesCountInPage;
                plottingHelper.ResetAxisWithMin(TimeSpan.FromMinutes(time));
                oxyHelper.DrawChart(graphics, model);

                oxyHelper.DrawChartTitles(graphics, page, _infoService.PdfRecordingSpeed);

                graphics.DrawHeader(page, hospital, researchTime, patient, doctor, _infoService.PregnancyStart, pregnancyDate);
                graphics.DrawPageNumbers(page, i + 1, pages);
                graphics.DrawDeviceSerialNumber(page, serialNumber);

                DrawDataWithMigradocNew(graphics, record, cardiografy, pregnancyDate);
                DrawComment(graphics, cardiografy);
                DrawBoxWithTime(graphics);
            }
        }

        private void DrawBoxWithTime(XGraphics graphics)
        {
            var document = new Document();
            var section = document.AddSection();
            var table = section.AddTable();

            var tableStyle = document.Styles.AddStyle(Style, StyleNames.Normal);
            tableStyle.Font.Size = PdfOrderConstants.HeaderFontSize;
            table.Format.Font.Name = PdfOrderConstants.FontName;
            table.Borders.Color = Colors.Black;

            table.Style = Style;
            table.Borders.Visible = true;
            table.Borders.Width = 0.75;
            table.Rows.LeftIndent = 5;

            table.AddColumn("1.5cm");
            var row = table.AddRow();
            row[0].FillCell(DateTime.Now.ToString("HH:mm"), ParagraphAlignment.Center);

            var docRenderer = new DocumentRenderer(document);
            docRenderer.PrepareDocument();
            graphics.MUH = PdfFontEncoding.Unicode;
            docRenderer.RenderObject(graphics, XUnit.FromMillimeter(43), XUnit.FromMillimeter(80), PdfOrderConstants.WidthA4, table);
        }

        private void DrawComment(XGraphics graphics, CardiotocographyInfo cardiotocography)
        {
            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            //
            var tableStyle = document.Styles.AddStyle(Style, StyleNames.Normal);
            tableStyle.Font.Size = PdfOrderConstants.HeaderFontSize;
            table.Format.Font.Name = PdfOrderConstants.FontName;
            table.Borders.Top.Clear();
            table.Borders.Left.Clear();
            table.Borders.Right.Clear();
            table.Borders.Bottom.Clear();
            //
            table.Style = Style;
            table.Borders.Visible = true;
            table.Rows.LeftIndent = 5;


            table.AddColumn("27.7cm");
            var t2row1 = table.AddRow();
            var countRoodDawsonCriteriaValid = cardiotocography.CountRoodDawsonCriteriaValid();

            var comment = countRoodDawsonCriteriaValid == 8
                ? AppStrings.PDF_DawsonRedmanCriteriaMet
                : string.Format(AppStrings.PDF_DawsonRedmanCriteriaNoMet, countRoodDawsonCriteriaValid);
            var cell = t2row1.Cells[0].AddParagraph(comment);
            cell.Format.Font.Bold = true;
            //  --------------------------

            var docRenderer = new DocumentRenderer(document);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            graphics.MUH = PdfFontEncoding.Unicode;
            docRenderer.RenderObject(graphics, XUnit.FromMillimeter(10), XUnit.FromMillimeter(70), PdfOrderConstants.WidthA4, table);
        }

        #region Построение таблицы
        public void DrawDataWithMigradocNew(XGraphics graphics, Record record, CardiotocographyInfo cardiografy, DateTime pregnancyStart)
        {
            var document = new Document();
            var section = document.AddSection();
            var table = section.AddTable();


            #region Общие настройки таблицы
            var tableStyle = document.Styles.AddStyle("MyTableStyle", StyleNames.Normal);
            tableStyle.Font.Size = PdfOrderConstants.DefaultFontSize;
            table.Format.Font.Name = PdfOrderConstants.FontName;
            table.Borders.Color = Colors.Black;

            table.Style = "MyTableStyle";
            table.Borders.Visible = true;
            table.Borders.Width = 0.75;
            table.Rows.LeftIndent = 5;
            #endregion


            #region Создаем колонки
            table.AddColumn("0.5cm");
            table.AddColumn("5cm");
            table.AddColumn("0.9cm");
            table.AddColumn("0.5cm");
            table.AddColumn("5cm");
            table.AddColumn("1.5cm");
            table.AddColumn("4cm");
            table.AddColumn("1.2cm");

            table.AddColumn("9cm");
            #endregion

            #region Создаем строки
            var row0 = BuildRow0(table, cardiografy);
            var row1 = BuildRow1(table, cardiografy, record.Biometric);
            var row2 = BuildRow2(table, cardiografy, record.Biometric);
            var row3 = BuildRow3(table, cardiografy, record.Biometric);
            var row4 = BuildRow4(table, cardiografy, record.Biometric);
            var row5 = BuildRow5(table, cardiografy, pregnancyStart);
            var row6 = BuildRow6(table, cardiografy);
            var row7 = BuildRow7(table, cardiografy);
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
            var (week, _) = pregnancyStart.CalculatePregnantTime();
            if (week >= Constants.BoundaryWeekOfTimeDependentParameters)
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

            var docRenderer = new DocumentRenderer(document);
            docRenderer.PrepareDocument();
            graphics.MUH = PdfFontEncoding.Unicode;
            docRenderer.RenderObject(graphics, XUnit.FromMillimeter(10), XUnit.FromMillimeter(25), PdfOrderConstants.WidthA4, table);
        }

        private Row BuildRow0(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            row.Cells[0].FillBoolCell(cardiotocography.IsValidRecordingDuration);
            row.Cells[1].FillCell(AppStrings.PDF_RecordingDurationMin10);
            row.Cells[2].FillCell(cardiotocography.RecordingDuration?.ToString("0.00") ?? PdfOrderConstants.DefultValue);

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
            row.Height = PdfOrderConstants.SizeRow;

            row.Cells[0].FillBoolCell(cardiotocography.SignalLossValid);
            row.Cells[1].FillCell(AppStrings.PDF_SignalLossMax20);
            row.Cells[2].FillCell(cardiotocography.SignalLossPercentage?.ToString(FloatFormat) ?? PdfOrderConstants.DefultValue);

            row.Cells[3].FillCell();
            row.Cells[4].FillCell(AppStrings.PDF_LTV);
            var textCell5 = cardiotocography.BeatLTV.HasValue && cardiotocography.TimeMsLTV.HasValue
                ? $"{cardiotocography.TimeMsLTV}({cardiotocography.BeatLTV})"
                : PdfOrderConstants.DefultDoubleValue;
            row.Cells[5].FillCell(textCell5);

            row.Cells[6].FillCell(AppStrings.PDF_TemperatureC);
            row.Cells[7].FillCell(biometric.Temperature.ToStringOrEmptyString(PdfOrderConstants.DefultValue, "0.0"));

            return row;
        }

        private Row BuildRow2(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            // ------
            row.Cells[0].FillBoolCell(cardiotocography.BasalHeartRateValid);
            row.Cells[1].FillCell(AppStrings.PDF_BasalFHRRange110_160);
            row.Cells[2].FillCell(cardiotocography.BasalHeartRate?.ToString(FloatFormat) ?? PdfOrderConstants.DefultValue);

            row.Cells[3].FillBoolCell(cardiotocography.STVValid);
            row.Cells[4].FillCell(AppStrings.PDF_STVMin4);
            row.Cells[5].FillCell(cardiotocography.STV?.ToString(FloatFormat) ?? PdfOrderConstants.DefultValue);

            row.Cells[6].FillCell(AppStrings.PDF_PulseBpm);
            row.Cells[7].FillCell(biometric.Pulse.ToStringOrEmptyString(PdfOrderConstants.DefultValue));
            // -----

            return row;
        }

        private Row BuildRow3(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            // -----

            row.Cells[1].FillCell(AppStrings.PDF_AccCountMore10);
            row.Cells[2].FillCell(cardiotocography.AccelerationsOver10?.ToString() ?? PdfOrderConstants.DefultValue);
            row.Cells[3].FillCell();

            row.Cells[4].FillCell(AppStrings.PDF_OscillationFrequency);
            row.Cells[5].FillCell(cardiotocography.OscillationFrequency?.ToString(FloatFormat) ?? PdfOrderConstants.DefultValue);

            row.Cells[6].FillCell(AppStrings.PDF_SugarMmolL);
            row.Cells[7].FillCell(biometric.Sugar.ToStringOrEmptyString(PdfOrderConstants.DefultValue));
            // ------

            return row;
        }

        private Row BuildRow4(Table table, CardiotocographyInfo cardiotocography, Biometric biometric)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            row.Cells[1].FillCell(AppStrings.PDF_AccCountMore15);
            row.Cells[2].FillCell(cardiotocography.AccelerationsOver15?.ToString() ?? PdfOrderConstants.DefultValue);

            row.Cells[3].FillBoolCell(cardiotocography.MovementFrequencyValid);

            row.Cells[4].FillCell(AppStrings.PDF_UCFrequencyMin3);
            row.Cells[5].FillCell(cardiotocography.MovementFrequency?.ToString(FloatFormat) ?? PdfOrderConstants.DefultValue);
            row.Cells[6].FillCell(AppStrings.PDF_HeartRate);
            var systolic = biometric?.Systolic.ToStringOrEmptyString(PdfOrderConstants.DefultValue);
            var diastolic = biometric?.Diastolic.ToStringOrEmptyString(PdfOrderConstants.DefultValue);
            row.Cells[7].FillCell($"{systolic}/{diastolic}");

            return row;
        }

        private Row BuildRow5(Table table, CardiotocographyInfo cardiotocography, DateTime pregnancyStart)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;
            // -----
            row.Cells[0].FillBoolCell(cardiotocography.DecelerationsMark);

            row.Cells[1].FillCell(AppStrings.PDF_DecCountMore20);//Кол-во Дец. > 20 уд./мин
            row.Cells[2].FillCell(cardiotocography.Decelerations?.ToString() ?? PdfOrderConstants.DefultValue);

            row.Cells[3].FillBoolCell(cardiotocography.IsTimeDependentParameters);

            row.Cells[4].FillCell(AppStrings.PDF_TimeDependentParameters);
            (var weeks, var days) = pregnancyStart.CalculatePregnantTime();
            row.Cells[5].FillCell($"{weeks}/{days}");

            // ------
            var cell6 = row.Cells[6];
            cell6.MergeRight = 2;

            return row;
        }

        private Row BuildRow6(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            row.Cells[1].FillCell(AppStrings.PDF_HighVariability);
            row.Cells[2].FillCell(cardiotocography.HighVariabilityMinutes?.ToString() ?? PdfOrderConstants.DefultValue);

            return row;
        }

        private Row BuildRow7(Table table, CardiotocographyInfo cardiotocography)
        {
            var row = table.AddRow();
            row.Height = PdfOrderConstants.SizeRow;

            row.Cells[1].FillCell(AppStrings.PDF_LowVariability);
            row.Cells[2].FillCell(cardiotocography.LowVariabilityMinutes?.ToString() ?? PdfOrderConstants.DefultValue);

            return row;
        }
        #endregion

        private string SettingsValueToPdf(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? AppStrings.PDF_NotSpecified
                : value;
        }
    }

}
