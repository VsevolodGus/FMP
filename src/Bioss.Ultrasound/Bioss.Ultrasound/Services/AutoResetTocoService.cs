using Bioss.Ultrasound.Ble.Devices;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Domain.Constants;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bioss.Ultrasound.Services
{
    public class AutoResetTocoService
    {
        private const byte MAX_TOCO = 100;
        private const long MAX_MILLISECONDS_EXCEEDED = 3000;

        private readonly IMyDevice _device;
        private readonly DeviceStreamProcessor _streamProcessor;
        private readonly AppSettingsService _appSettingsService;
        private readonly Stopwatch _stopwatch = new();

        public AutoResetTocoService(IMyDevice device,
            DeviceStreamProcessor streamProcessor,
            AppSettingsService appSettingsService)
        {
            _device = device;
            _streamProcessor = streamProcessor;
            _appSettingsService = appSettingsService;
            _streamProcessor.PackageReady += OnNewPackage;
        }
           
        private async Task OnNewPackage(Package package)
        {
            if (!_appSettingsService.IsAutoResetToco)
                return;

            var fhr = package.FHRPackage;
            if (fhr == null)
                return;

            //  Если TOCO превышает порог, то увеличиваем счетчик, иначе счетчик можно сбросить
            var toco = fhr.Toco;
            if (toco >= MAX_TOCO)
            {
                if (!_stopwatch.IsRunning)
                    _stopwatch.Restart();
            }
            else
                _stopwatch.Stop();


            //Debug.WriteLine($"ElapsedMilliseconds: {_stopwatch.ElapsedMilliseconds}");

            //  Если TOCO превышает порог несколько секунд, то сбросим его
            if (_stopwatch.IsRunning && _stopwatch.ElapsedMilliseconds >= MAX_MILLISECONDS_EXCEEDED)
            {
                _stopwatch.Stop();
                await _device.ResetTocoAsync();
                MessagingCenter.Send<object>(this, MessagingCenterConstants.TocoReseting);
            }
        }
    }
}
