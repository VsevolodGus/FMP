using Android.Content;
using Android.Media;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Stream = Android.Media.Stream;
using PlatformApp = Android.App;


namespace Bioss.Ultrasound.Maui.Platforms.Android.Extensions;

public partial class SystemVolume : ISystemVolume
{
    private readonly Stream StreamType = Stream.Music;
    private readonly CustomMediaRouterCallback _mediaRouterCallback = new();
    private readonly AudioManager _audioManager;
    private readonly MediaRouter _mediaRouter;

    public SystemVolume()
    {
        var context = PlatformApp.Application.Context;
        _audioManager = (AudioManager)context.GetSystemService(Context.AudioService);

        _mediaRouter = (MediaRouter)context.GetSystemService(Context.MediaRouterService);
        _mediaRouter.AddCallback(MediaRouteType.LiveAudio, _mediaRouterCallback);

        _mediaRouterCallback.VolumeChanged += OnVolumeChanged;
    }

    public event EventHandler<double> VolumeChanged;

    public double Volume
    {
        get => GetVolume(_audioManager.GetStreamVolume(StreamType));
        set => _audioManager.SetStreamVolume(StreamType, GetVolumeIndex(value), VolumeNotificationFlags.Vibrate);
    }

    private void OnVolumeChanged(object sender, MediaRouter.RouteInfo info)
    {
        if (info.PlaybackStream != StreamType)
            return;

        var volume = GetVolume(info.Volume);
        VolumeChanged?.Invoke(this, volume);
    }

    private int GetVolumeIndex(double volume)
    {
        return (int)(volume * GetVolumeIndexLength());
    }

    private double GetVolume(int index)
    {
        var volume = (double)index / GetVolumeIndexLength();
        return Math.Round(volume, 1);
    }

    private int GetVolumeIndexLength()
    {
        var maxVolume = _audioManager.GetStreamMaxVolume(StreamType);
        var minVolume = _audioManager.GetStreamMinVolume(StreamType);
        return (maxVolume - minVolume);
    }
}

public partial class SystemVolume
{
    private class CustomMediaRouterCallback : MediaRouter.SimpleCallback
    {
        internal event EventHandler<MediaRouter.RouteInfo> VolumeChanged;

        public override void OnRouteVolumeChanged(MediaRouter router, MediaRouter.RouteInfo info)
        {
            VolumeChanged?.Invoke(router, info);
        }
    }
}
