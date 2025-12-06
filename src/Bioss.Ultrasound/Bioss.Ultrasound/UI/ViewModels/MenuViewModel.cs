using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.UI.Pages;
using Libs.DI.ViewModels;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;

        public MenuViewModel(INavigation navigation)
        {
            _navigation = navigation;

            Settings = new List<Setting>
            {
                new Setting
                {
                    Name = AppStrings.Menu_Settings,
                    Description = AppStrings.Menu_SettingsDescription,
                    Action = async () => await _navigation.PushAsync(new SettingsPage())
                },
                new Setting
                {
                    Name = AppStrings.Menu_About,
                    Description = AppStrings.Menu_AboutDescription,
                    Action = async () => await _navigation.PushAsync(new AboutPage())
                },
                //new Setting
                //{
                //    Name = "Backup",
                //    Description = AppStrings.Menu_AboutDescription,
                //    Action = async () => await _navigation.PushAsync(new BackupPage())
                //},
            };
        }

        public List<Setting> Settings { get; }

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
}
