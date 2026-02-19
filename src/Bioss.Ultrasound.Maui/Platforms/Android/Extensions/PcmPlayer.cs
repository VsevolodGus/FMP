using Android.Media;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Stream = Android.Media.Stream;

namespace Bioss.Ultrasound.Maui.Platforms.Android.Extensions;

public class PcmPlayer : IPcmPlayer
{
    private const int AMPLIFIER = 60;
    private AudioTrack _audio;

    public void AddSound(short[] sound)
    {
        for (var i = 0; i < sound.Length; ++i)
            sound[i] = (short)GainedValue(sound[i]);

        _audio.Write(sound, 0, sound.Length);
    }

    public void Init()
    {
        int byfferSize = AudioTrack.GetMinBufferSize(4000, ChannelOut.Mono, Encoding.Pcm16bit);
        _audio = new AudioTrack(Stream.Music, 4000, ChannelOut.Mono, Encoding.Pcm16bit, byfferSize, AudioTrackMode.Stream);
    }

    public void Start()
    {
        _audio.Play();
    }

    private int GainedValue(int val)
    {
        var gainedValue = val * AMPLIFIER;
        if (gainedValue < -32768)
        {
            gainedValue = -32768;
        }
        else if (gainedValue > 32767)
        {
            gainedValue = 32767;
        }

        return gainedValue;
    }
}