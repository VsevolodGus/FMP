using System;
namespace Bioss.Ultrasound.DependencyExtensions
{
    public interface IPcmPlayer
    {
        void AddSound(Int16[] sound);
        void Init();
        void Start();
    }
}
