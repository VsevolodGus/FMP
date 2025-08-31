using System;

namespace Bioss.Ultrasound.DependencyExtensions
{
    public interface ISystemVolume
    {
        double Volume { get; set; }
        event EventHandler<double> VolumeChanged;
    }
}
