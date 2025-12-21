using System;
using System.Diagnostics;
using System.Linq;
using Bioss.Ultrasound.DependencyExtensions;
using MediaPlayer;
using UIKit;

namespace Bioss.Ultrasound.iOS.Extensions
{
    public class SystemVolume : ISystemVolume
    {
        private readonly UISlider _slider;

        public SystemVolume()
        {
            var volumeView = new MPVolumeView();
            _slider = volumeView.Subviews.FirstOrDefault(a => a is UISlider) as UISlider;

            if (_slider != null)
                _slider.ValueChanged += OnValueChanged;
        }

        public double Volume
        {
            get
            {
                if (_slider is null)
                    return .0;

                var volume = _slider.Value;
                return Math.Round(volume, 1);
            }

            set
            {
                if (_slider is null)
                    return;

                _slider.Value = (float)value;
                VolumeChanged?.Invoke(this, value);
            }
        }

        public event EventHandler<double> VolumeChanged;

        private void OnValueChanged(object sender, EventArgs e)
        {
            var slider = sender as UISlider;
            double volume = _slider.Value;
            volume = Math.Round(volume, 2);
            VolumeChanged?.Invoke(this, volume);

            Debug.WriteLine($"Volume {volume}");
        }
    }
}
