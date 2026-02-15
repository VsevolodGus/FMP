using Bioss.Ultrasound.Core.Ble.Devices;
using Bioss.Ultrasound.Core.Ble.Models;
using Bioss.Ultrasound.Core.Domain.Constants;
using System.Diagnostics;

namespace Bioss.Ultrasound.Services
{
    public class AutoResetTocoService
    {
        private const byte MAX_TOCO = 100;
        private const long MAX_MILLISECONDS_EXCEEDED = 3000;
        private readonly IMyDevice _device;

        private Stopwatch _stopwatch = new Stopwatch();

        public AutoResetTocoService(IMyDevice device)
        {
            _device = device;
            _device.NewPackage += OnNewPackage;
        }

        public bool IsAutoResetToco { get; set; }
        
        
        private async void OnNewPackage(object sender, Package package)
        {
            if (!IsAutoResetToco)
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
