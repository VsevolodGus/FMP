namespace Bioss.Ultrasound.Core.DependencyExtensions;

public interface ISystemVolume
{
    double Volume { get; set; }
    event EventHandler<double> VolumeChanged;
}
