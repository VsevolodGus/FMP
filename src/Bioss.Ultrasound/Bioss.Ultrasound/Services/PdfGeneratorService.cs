using System;
using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Domain.Plotting;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Abstracts;
using Bioss.Ultrasound.Tools;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using static Bioss.Ultrasound.Services.PdfGeneratorService2;

namespace Bioss.Ultrasound.Services
{
    //  TODO: это не сервис. Придумать кокому слою он пренадлежит
    public class PdfGeneratorService : IPdfGenerator
    {
        private readonly InfoSettingsService _infoService;

        public PdfGeneratorService(InfoSettingsService infoService)
        {
            _infoService = infoService;
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
                        ? $", {string.Format(AppStrings.PDF_UserBirthdayDescriptionAge, DateTools.CalculateAge(_infoService.Birthday.Value))}"
                        : "";
            var patient = _infoService.IsPersonalDevice
                            ? $"{_infoService.Patient ?? "-"}{age}"
                            : AppStrings.PDF_NotSpecified;
            var researchTime = record.StartTime;

            var serialNumber = record.DeviceSerialNumber ?? "";

            var fetalsCount = record.Events.Count(a => a.Event == Events.FetalMovement);

            //  сколько секунд отображать на одной странице
            var oxyHelper = new OxyPdfHelper();

            double maxSantimetersCanDisplay = oxyHelper.ChartLength.Centimeter;
            int santimetersInMinute = _infoService.PdfRecordingSpeed;
            double minutesCountInPage = maxSantimetersCanDisplay / santimetersInMinute;

            var count = (int)record.RecordingTimeSpan.TotalMinutes;
            int pages = ((count - 1) / (int)minutesCountInPage) + 1;

            var helper = new Helper();

            var plottingHelper = new PlottingHelper();
            plottingHelper.Scale = minutesCountInPage * 60;

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

                helper.DrawHeader(graphics, page, hospital, researchTime, patient, doctor);
                helper.DrawPageNumbers(graphics, page, i + 1, pages);
                helper.DrawDeviceSerialNumber(graphics, page, serialNumber);

                //helper.DrawRullerTest(graphics, XUnit.FromMillimeter(22 + 260), XUnit.FromMillimeter(2.5));
                //helper.DrawHorizontalRullerTest(graphics, XUnit.FromMillimeter(22), XUnit.FromMillimeter(30));
                //helper.DrawVerticalRullerTest(graphics, XUnit.FromMillimeter(45), XUnit.FromMillimeter(2.5));
                //helper.DrawVerticalRullerTest(graphics, XUnit.FromMillimeter(86), XUnit.FromMillimeter(9));

                DrawDataWithMigradoc(graphics, record, fetalsCount, _infoService.PregnancyStart);
                DrawDoctorComment(graphics);
            }
        }

        private string SettingsValueToPdf(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? AppStrings.PDF_NotSpecified
                : value;
        }

        private void DrawDataWithMigradoc(XGraphics graphics, Record record, int fetalsCount, DateTime? pregnancyStart)
        {
            var biometric = record.Biometric ?? new Biometric();
            var recordTime = record.RecordingTimeSpan;
            var researchTime = record.StartTime;

            // table
            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();

            //
            var tableStyle = document.Styles.AddStyle("MyTableStyle", StyleNames.Normal);
            tableStyle.Font.Size = 9;
            table.Format.Font.Name = "Segoe UI";
            table.Borders.Color = Colors.LightGray;
            //

            table.Style = "MyTableStyle";
            table.Borders.Visible = true;
            table.Borders.Width = 0.75;
            table.Rows.LeftIndent = 5;

            var column = table.AddColumn("5cm");
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            //  empty column
            column = table.AddColumn("0.5cm");
            column.Borders.Top.Clear();
            column.Borders.Bottom.Clear();

            column = table.AddColumn("4cm");
            column = table.AddColumn("1.5cm");
            column.Format.Alignment = ParagraphAlignment.Right;

            //  empty column
            column = table.AddColumn("0.5cm");
            column.Borders.Top.Clear();
            column.Borders.Bottom.Clear();

            column = table.AddColumn("14.7cm");

            var empty = "-";

            //  --------------------------

            var row0 = table.AddRow();
            row0.Cells[0].AddParagraph(AppStrings.PDF_TableCTGParameters);
            row0.Cells[0].MergeRight = 1;

            row0.Cells[3].AddParagraph(AppStrings.PDF_TableMotherParameters);
            row0.Cells[3].MergeRight = 1;

            row0.Cells[6].AddParagraph(AppStrings.PDF_Comment);

            var row1 = table.AddRow();

            row1.Cells[0].AddParagraph($"{AppStrings.PDF_RecordingDuration}, {AppStrings.Unit_min}");
            row1.Cells[1].AddParagraph($"{recordTime.Minutes}");

            row1.Cells[3].AddParagraph($"{AppStrings.PDF_Temperature}, {AppStrings.Unit_Celsius}");
            row1.Cells[4].AddParagraph($"{StringTools.ToStringOrEmptyString(biometric.Temperature, empty)}");

            //  Убираем переносы строк в тексте комментария
            var comment = (biometric?.Comment ?? "").Replace("\n", " ").Replace("  ", " ");
            row1.Cells[6].AddParagraph(comment);
            row1.Cells[6].MergeDown = 3;
            //  --------------------------

            var row2 = table.AddRow();

            row2.Cells[0].AddParagraph($"{AppStrings.PDF_FetalMovements}");
            row2.Cells[1].AddParagraph($"{fetalsCount}");

            row2.Cells[3].AddParagraph($"{AppStrings.PDF_Pulse}, {AppStrings.Unit_HeartRate}");
            row2.Cells[4].AddParagraph($"{StringTools.ToStringOrEmptyString(biometric.Pulse, empty)}");

            //  --------------------------

            var row3 = table.AddRow();

            row3.Cells[0].AddParagraph($"{AppStrings.PDF_GestationalAge}");

            var gestationAge = empty;
            if (pregnancyStart.HasValue)
            {
                var time = DateTools.CalculatePregnantTime(pregnancyStart.Value, researchTime);
                gestationAge = $"{time.weeks}/{time.days}";
            }
            row3.Cells[1].AddParagraph(gestationAge);

            row3.Cells[3].AddParagraph($"{AppStrings.PDF_Sugar}, {AppStrings.Unit_BloodGlucose}");
            row3.Cells[4].AddParagraph($"{StringTools.ToStringOrEmptyString(biometric.Sugar, empty)}");

            
            //  --------------------------

            var row4 = table.AddRow();

            row4.Cells[0].AddParagraph($"{AppStrings.PDF_SignaLoss}, %");

            var signalLoss = Math.Round(record.LossPercentage * 100, 1);
            row4.Cells[1].AddParagraph($"{signalLoss}");

            row4.Cells[3].AddParagraph($"{AppStrings.PDF_HeartRate}, {AppStrings.Unit_MillimetreOfMercury}");
            row4.Cells[4].AddParagraph($"{StringTools.ToStringOrEmptyString(biometric.Systolic, empty)}/{StringTools.ToStringOrEmptyString(biometric.Diastolic, empty)}");

            //  --------------------------


            var docRenderer = new DocumentRenderer(document);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            graphics.MUH = PdfFontEncoding.Unicode;
            docRenderer.RenderObject(graphics, XUnit.FromMillimeter(10), XUnit.FromMillimeter(25), "28cm", table);
        }

        private void DrawDoctorComment(XGraphics graphics)
        {
            var document = new Document();
            var section = document.AddSection();

            var table = section.AddTable();
            //
            var tableStyle = document.Styles.AddStyle("MyTableStyle2", StyleNames.Normal);
            tableStyle.Font.Size = 9;
            table.Format.Font.Name = "Segoe UI";
            table.Borders.Color = Colors.Gray;
            table.Borders.Top.Clear();
            table.Borders.Left.Clear();
            table.Borders.Right.Clear();
            table.Borders.Bottom.Width = 0.5;
            //
            table.Style = "MyTableStyle2";
            table.Borders.Visible = true;
            table.Rows.LeftIndent = 5;


            table.AddColumn("27.7cm");
            var t2row1 = table.AddRow();
            t2row1.Cells[0].AddParagraph(AppStrings.PDF_DoctorsConclusion);
            table.AddRow();
            table.AddRow();
            //  --------------------------

            var docRenderer = new DocumentRenderer(document);
            docRenderer.PrepareDocument();

            // Render the paragraph. You can render tables or shapes the same way.
            graphics.MUH = PdfFontEncoding.Unicode;
            docRenderer.RenderObject(graphics, XUnit.FromMillimeter(10), XUnit.FromMillimeter(50), "28cm", table);
        }

    }
}
