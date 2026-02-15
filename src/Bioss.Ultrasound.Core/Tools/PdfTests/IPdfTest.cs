namespace Bioss.Ultrasound.Tools.PdfTests
{
    public interface IPdfTest
    {
        string Name { get; }
        void CreatePdfFile(string fileName);
    }
}
