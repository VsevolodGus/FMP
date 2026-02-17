using Bioss.Ultrasound.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bioss.Ultrasound.Maui.Pages;
using Bioss.Ultrasound.Maui.Navigation;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public AboutViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    public string Version => AppConstants.AppVersion;

    [RelayCommand]
    public async Task PrivacyCommand()
        => await _navigation.NavigateModalAsync<DocumentPage>();

    [RelayCommand]
    public async Task OpenInstruction()
        => await Browser.OpenAsync("https://bipuls.ru/");
    
}
