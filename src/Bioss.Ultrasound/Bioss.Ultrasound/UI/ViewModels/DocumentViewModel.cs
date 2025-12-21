using Bioss.Ultrasound.Resources.Localization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class DocumentViewModel
    {
        private readonly INavigation _navigation;
        private readonly static string TextPrivacyPolicy;

        public string Text => TextPrivacyPolicy;

        public DocumentViewModel(INavigation navigation)
        {
            _navigation = navigation;
        }

        public ICommand Close => new Command(async a =>
        {
            await _navigation.PopModalAsync();
        });

        static DocumentViewModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(AppStrings.DocPrivacy));

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                TextPrivacyPolicy = reader.ReadToEnd();
            }
        }
    }
}
