using System;
using System.Globalization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Tools;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace Bioss.Ultrasound.UI.Popups
{
    public partial class BiometricPopup : PopupPage
    {
        private readonly IUserDialogs _dialogs;
        private Biometric _biometric;
        private EntryData _vm;
        
        private TaskCompletionSource<BiometricPopupResult> _taskCompletionSource = new TaskCompletionSource<BiometricPopupResult>();
        public Task<BiometricPopupResult> PopupClosedTask => _taskCompletionSource.Task;

        public BiometricPopup(IUserDialogs dialogs, Biometric biometric)
        {
            InitializeComponent();

            _dialogs = dialogs;
            _biometric = biometric;

            _vm = new EntryData
            {
                Comment = _biometric.Comment,
                Temperature = StringTools.ToStringOrEmptyString(_biometric.Temperature),
                Sugar = StringTools.ToStringOrEmptyString(_biometric.Sugar),
                Diastolic = StringTools.ToStringOrEmptyString(_biometric.Diastolic),
                Systolic = StringTools.ToStringOrEmptyString(_biometric.Systolic),
                Pulse = StringTools.ToStringOrEmptyString(_biometric.Pulse),
            };

            BindingContext = _vm;
        }

        private async void SubmitButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var temperature = ParseDouble(_vm.Temperature);
                var sugar = ParseDouble(_vm.Sugar);
                var diastolic = (int)ParseDouble(_vm.Diastolic); // обрезаем то что после запятой
                var systolic = (int)ParseDouble(_vm.Systolic);   // обрезаем то что после запятой
                var pulse = (int)ParseDouble(_vm.Pulse);         // обрезаем то что после запятой
                var comment = _vm.Comment;

                //  return data
                _biometric.Temperature = temperature;
                _biometric.Sugar = sugar;
                _biometric.Diastolic = diastolic;
                _biometric.Systolic = systolic;
                _biometric.Pulse = pulse;
                _biometric.Comment = comment;

                await DismissAsync(new BiometricPopupResult(true, _biometric));
            }
            catch
            {
                await _dialogs.AlertAsync(AppStrings.BiometricPopup_ErrorEnter);
            }
        }

        private async Task DismissAsync(BiometricPopupResult result)
        {
            await PopupNavigation.Instance.PopAsync();
            _taskCompletionSource.SetResult(result);
        }

        private async void CancelButtonClicked(object sender, EventArgs e)
        {
            await DismissAsync(new BiometricPopupResult(false, _biometric));
        }

        private double ParseDouble(string value)
        {
            //  Позволяем оставить пустое поле. Это значит,
            //  что пользователь не захотел вводить этот параметр
            //  и в БД сохраним значение 0
            if (string.IsNullOrWhiteSpace(value))
                return .0;

            return double.Parse(value.Replace(',', '.'), CultureInfo.GetCultureInfo("en-US"));
        }

        class EntryData
        {
            public string Comment { get; set; }
            public string Temperature { get; set; }
            public string Sugar { get; set; }
            public string Systolic { get; set; }
            public string Diastolic { get; set; }
            public string Pulse { get; set; }
        }
    }

    public class BiometricPopupResult
    {
        public BiometricPopupResult(bool ok, Biometric biometric)
        {
            Ok = ok;
            Biometric = biometric;
        }

        public bool Ok { get; }
        public Biometric Biometric { get; }
    }
}
