//using Bioss.Ultrasound.Core.Domain.Models;
//using Bioss.Ultrasound.Core.Tools;
//using Bioss.Ultrasound.Domain.Plotting;
//using PdfSharpCore.Drawing;
//using PdfSharpCore.Pdf;
//using PdfSharpCore.Pdf.IO;

//namespace Bioss.Ultrasound.Tools.PdfTests
//{
//    public class PdfSharpOxyPlotTest : IPdfTest
//    {
//        private readonly Record _record;

//        public PdfSharpOxyPlotTest(Record record)
//        {
//            _record = record;
//        }

//        public string Name => "PdfSharp - OxyPlot";

//        public void CreatePdfFile(string fileName)
//        {
//            MyFontResolver.Apply();

//            var document = new PdfDocument();
//            document.Info.Title = "PDFsharp XGraphic Sample";
//            document.Info.Author = "Stefan Lange";
//            document.Info.Subject = "Created with code snippets that show the use of graphical functions";
//            document.Info.Keywords = "PDFsharp, XGraphics";

//            // Create demonstration pages
//            new OxyImage(_record).DrawPage(document, document.AddPage());

//            // Save the s_document...
//            document.Save(fileName);
//        }

//        class OxyImage
//        {
//            private readonly Record _record;

//            public OxyImage(Record record)
//            {
//                _record = record;
//            }

//            public void DrawPage(PdfDocument document, PdfPage page)
//            {
//                XGraphics gfx = XGraphics.FromPdfPage(page);

//                DrawHelper.DrawTitle(document, page, gfx, "Lines & Curves");

//                DrawFormXObject(gfx, 8);
//            }

//            void DrawFormXObject(XGraphics gfx, int number)
//            {
//                var chartFileName = Path.Combine(Path.GetTempPath(), "OxyPdf.pdf");
//                GenerateObjectFile(chartFileName);


//                XImage image = XImage.FromFile(chartFileName);

//                const double dx = 10, dy = 10;

//                double width = image.PixelWidth * 72 / image.HorizontalResolution;
//                double height = image.PixelHeight * 72 / image.HorizontalResolution;

//                gfx.DrawImage(image, dx, dy, width, height);
//            }

//            private void GenerateObjectFile(string fileName)
//            {
//                var model = GetPlot();
//                model.DefaultFont = "Segoe UI";

//                var fileName2 = Path.GetTempFileName();
//                using (var stream = File.OpenWrite(fileName2))
//                {
//                    //exporter.Export(model, stream);
//                    Export(model, stream, 400, 400, OxyColors.White);
//                }

//                // FIX oxyplot exporting to pdf
//                var doc = PdfReader.Open(fileName2);
//                doc.Save(fileName);
//            }

//            private PlotModel GetPlot()
//            {
//                var chartDrawer = new ChartDrawer(new PlottingHelper());

//                chartDrawer.Fill(_record);

//                return chartDrawer.Model;
//            }

//            public static void Export(IPlotModel model, Stream stream, double width, double height, OxyColor background)
//            {
//                var exporter = new PdfExporter { Width = width, Height = height, Background = background };
//                exporter.Export(model, stream);
//            }
//        }
//    }
//}
