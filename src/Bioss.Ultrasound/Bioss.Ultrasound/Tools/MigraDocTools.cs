using MigraDocCore.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using PdfSharpCore.Utils;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using System.Reflection;

namespace Bioss.Ultrasound.Tools
{
    public class MigraDocTools
    {
        //https://githubmemory.com/repo/ststeiger/PdfSharpCore/issues/96
        public static ImageSource.IImageSource GetImageSourceFromResources(string imageName, string extension = "png")
        {
            if (ImageSource.ImageSourceImpl == null)
                ImageSource.ImageSourceImpl = new ImageSharpImageSource<Rgba32>();

            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly
                .GetManifestResourceNames()
                .Single(str => str.EndsWith($"Resources.Images.{imageName}.{extension}"));

            return ImageSource.FromStream(imageName, () => assembly.GetManifestResourceStream(resourceName));
        }
    }
}
