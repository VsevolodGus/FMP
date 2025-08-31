using OxyPlot;
using OxyPlot.Annotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bioss.Ultrasound.Tools
{
    public class OxyTools
    {
        public static ImageAnnotation MakeImageAnnotation(double x, double y, string imageName, string key = null, int width = 20)
        {
            return new ImageAnnotation
            {
                ImageSource = new OxyImage(LoadImage(imageName)),
                X = new PlotLength(x, PlotLengthUnit.Data),
                Y = new PlotLength(y, PlotLengthUnit.Data),
                Width = new PlotLength(width, PlotLengthUnit.ScreenUnits),
                YAxisKey = key,
            };
        }

        private static byte[] LoadImage(string imageName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly
                .GetManifestResourceNames()
                .Single(str => str.EndsWith($"Resources.Images.{imageName}.png"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                var length = (int)stream.Length;
                byte[] data = new byte[length];
                stream.Read(data, 0, length);
                return data;
            }
        }
    }
}
