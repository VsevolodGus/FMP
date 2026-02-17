namespace Bioss.Ultrasound.Core.DependencyExtensions;

public interface IPcmPlayer
{
    void AddSound(short[] sound);
    void Init();
    void Start();
}
