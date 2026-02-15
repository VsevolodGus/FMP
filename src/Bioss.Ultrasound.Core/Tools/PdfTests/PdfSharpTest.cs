using Bioss.Ultrasound.Core.Tools;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Bioss.Ultrasound.Tools.PdfTests
{
    public class PdfSharpTest : IPdfTest
    {
        public string Name => "PdfSharp";

        public void CreatePdfFile(string fileName)
        {
            MyFontResolver.Apply();

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Segoe UI", 20);

            gfx.DrawString("Test of PdfSharp", font, new XSolidBrush(XColor.FromArgb(0, 0, 0)), 10, 130);


            //
            var pen = new XPen(XColors.Black);
            gfx.DrawLine(pen, 45, 250, 45, 703);
            gfx.DrawLine(pen, 87, 250, 87, 703);
            gfx.DrawLine(pen, 150, 250, 150, 703);
            gfx.DrawLine(pen, 291, 250, 291, 703);
            gfx.DrawLine(pen, 381, 250, 381, 703);
            gfx.DrawLine(pen, 461, 250, 461, 703);
            gfx.DrawLine(pen, 571, 250, 571, 703);


            //
            document.Save(fileName);
        }
    }
}
