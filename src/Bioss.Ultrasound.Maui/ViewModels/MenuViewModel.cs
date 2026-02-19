using Bioss.Ultrasound.Core.Resources.Localization;
using Bioss.Ultrasound.Maui.Navigation;
using Bioss.Ultrasound.Maui.Pages;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bioss.Ultrasound.Maui.ViewModels;

public class MenuViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public IReadOnlyList<Setting> Settings { get; }
    public MenuViewModel(INavigationService navigation)
    {
        _navigation = navigation;

        Settings = new List<Setting>
        {
            new Setting
            {
                Name = AppStrings.Menu_Settings,
                Description = AppStrings.Menu_SettingsDescription,
                Action = async () => await _navigation.NavigateToAsync<SettingsPage>()
            },
            new Setting
            {
                Name = AppStrings.Menu_About,
                Description = AppStrings.Menu_AboutDescription,
                Action = async () => await _navigation.NavigateToAsync<AboutPage>()
            },
            //new Setting
            //{
            //    Name = "Backup",
            //    Description = AppStrings.Menu_AboutDescription,
            //    Action = async () => await _navigation.NavigateToAsync<BackupPage>()
            //},
        };
    }

  

    public Setting SelectedSetting
    {
        get => null;
        set
        {
            OnPropertyChanged();
            value?.Action();
        }
    }
}

public class Setting
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Action Action { get; set; }
}
