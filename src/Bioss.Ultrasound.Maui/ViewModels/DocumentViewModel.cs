using Bioss.Ultrasound.Core.Resources.Localization;
using Bioss.Ultrasound.Maui.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class DocumentViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly static string TextPrivacyPolicy;

    public string Text => TextPrivacyPolicy;

    public DocumentViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task Close()
        => await _navigation.CloseModelAsync();

    static DocumentViewModel()
    {
        var assembly = typeof(AppStrings).Assembly;
        //var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(AppStrings.DocPrivacy));

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        TextPrivacyPolicy = reader.ReadToEnd();
    }
}
