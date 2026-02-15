using System.Reflection;
using Plugin.SimpleAudioPlayer;

namespace Bioss.Ultrasound.Core.Services;

public enum Sounds
{
    Attention,
    LowBattery,
    LossData
}

public class AudioService
{
    private const string audioResourceName = "Bioss.Ultrasound.Resources.Audio.MixkitAlarmTone996.wav";
    private readonly Dictionary<Sounds, SoundPlayer> _players = new()
    {
        { Sounds.Attention, new SoundPlayer(audioResourceName) },
        { Sounds.LowBattery, new SoundPlayer(audioResourceName) },
        { Sounds.LossData, new SoundPlayer(audioResourceName) }
    };

    public void Play(in Sounds sound, in bool loop = false)
    {
        _players[sound].Play(loop);
    }

    public void Stop(in Sounds sound)
    {
        _players[sound].Stop();
    }

    public void Stop()
    {
        foreach (var player in _players)
            player.Value.Stop();
    }

    class SoundPlayer
    {
        private ISimpleAudioPlayer _player { get; }

        public SoundPlayer(string audioResourceName)
        {
            _player = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            _player.Load(GetResourceStream(audioResourceName));
            _player.PlaybackEnded += OnPlaybackEnded;
        }

        //  используем собственное свойство Loop, так как одноименное свойстово в ISimpleAudioPlayer не работало
        public bool Loop { get; set; }

        public void Play(in bool loop)
        {
            Loop = loop;
            _player.Play();
        }

        public void Stop()
        {
            Loop = false;
            _player.Stop();
        }

        private void OnPlaybackEnded(object sender, EventArgs e)
        {
            if (Loop)
                _player.Play();
        }

        private static Stream GetResourceStream(string resource)
        {
            // TODO понять то-ли юзаем
            var assembly = typeof(Sounds).GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(resource);
        }
    }
}
