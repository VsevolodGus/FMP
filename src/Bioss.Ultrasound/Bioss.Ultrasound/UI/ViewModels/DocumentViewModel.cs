using System.IO;
using System.Linq;
using System.Reflection;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class DocumentViewModel
    {
        public DocumentViewModel(string documentName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(documentName));

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    Text = reader.ReadToEnd();
                }
            }
            catch { }
        }

        public string Text { get; set; }
    }
}
