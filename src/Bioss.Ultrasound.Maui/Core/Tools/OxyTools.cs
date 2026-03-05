//using OxyPlot;
//using OxyPlot.Annotations;
//using System.Reflection;

//namespace Bioss.Ultrasound.Core.Tools;

//public class OxyTools
//{
//    // capacity = 7, потому что всего 7 картинок
//    private readonly static Dictionary<string, OxyImage> _cacheImage = new Dictionary<string, OxyImage>(7);

//    public static ImageAnnotation MakeImageAnnotation(in double x, in double y, in string imageName, in string key = null, in int width = 20)
//    {
//        if (!_cacheImage.TryGetValue(imageName, out var image))
//        {
//            image = new OxyImage(LoadImage(imageName));
//            _cacheImage.TryAdd(imageName, image);
//        }

//        return new ImageAnnotation
//        {
//            ImageSource = image,
//            X = new PlotLength(x, PlotLengthUnit.Data),
//            Y = new PlotLength(y, PlotLengthUnit.Data),
//            Width = new PlotLength(width, PlotLengthUnit.ScreenUnits),
//            YAxisKey = key,
//        };
//    }

//    private static byte[] LoadImage(string imageName)
//    {
//        var assembly = Assembly.GetExecutingAssembly();
//        string resourceName = assembly
//            .GetManifestResourceNames()
//            .Single(str => str.EndsWith($"Resources.Images.{imageName}.png"));

//        using Stream stream = assembly.GetManifestResourceStream(resourceName);
//        var length = (int)stream.Length;
//        byte[] data = new byte[length];
//        stream.Read(data, 0, length);
//        return data;
//    }
//}
