using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Plugin.SimpleAudioPlayer;

namespace Bioss.Ultrasound.Services
{
    public partial class AudioService
    {
        private readonly Dictionary<Sounds, SoundPlayer> _players = new()
        {
            { Sounds.Attention, new SoundPlayer("Bioss.Ultrasound.Resources.Audio.MixkitAlarmTone996.wav") },
            { Sounds.LowBattery, new SoundPlayer("Bioss.Ultrasound.Resources.Audio.MixkitAlarmTone996.wav") },
            { Sounds.LossData, new SoundPlayer("Bioss.Ultrasound.Resources.Audio.MixkitAlarmTone996.wav") }
        };

        public void Play(Sounds sound, bool loop = false)
        {
            _players[sound].Play(loop);
        }

        public void Stop(Sounds sound)
        {
            _players[sound].Stop();
        }

        public void Stop()
        {
            foreach (var player in _players)
                player.Value.Stop();
        }
    }

    public partial class AudioService
    {
        public enum Sounds
        {
            Attention,
            LowBattery,
            LossData
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

            public void Play(bool loop)
            {
                Loop = loop;
                _player.Play();
            }

            public void Stop()
            {
                Loop = false;
                _player.Stop();
            }

            private void OnPlaybackEnded(object sender, System.EventArgs e)
            {
                if (Loop)
                    _player.Play();
            }

            private Stream GetResourceStream(string resource)
            {
                var assembly = typeof(App).GetTypeInfo().Assembly;
                return assembly.GetManifestResourceStream(resource);
            }
        }
    }
}
