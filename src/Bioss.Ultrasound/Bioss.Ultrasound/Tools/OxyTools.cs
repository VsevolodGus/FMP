using OxyPlot;
using OxyPlot.Annotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bioss.Ultrasound.Tools
{
    public class OxyTools
    {
        public static ImageAnnotation MakeImageAnnotation(in double x, in double y, in string imageName, in string key = null, in int width = 20)
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

        // TODO кешировать бы тут все изображения, а не каждый раз читать с диска
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
