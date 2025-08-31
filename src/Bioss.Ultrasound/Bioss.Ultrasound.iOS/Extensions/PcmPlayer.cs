using System;
using Bioss.Ultrasound.Collections;
using Bioss.Ultrasound.DependencyExtensions;
using AudioUnit;
using Foundation;
using AVFoundation;
using System.Diagnostics;
using AudioToolbox;

namespace Bioss.Ultrasound.iOS.Extensions
{
    class AudioData
    {
        public RingBuffer<short> Buffer { get; } = new RingBuffer<short>(1024);
        public int Amplifier { get; set; } = 60;
    }

    public class PcmPlayer : IPcmPlayer
    {
        private AudioData _audioData = new AudioData();

        private AVAudioSession _audioSession;
        private AudioUnit.AudioUnit _audioUnit;

        private NSObject _notification;

        public void Init()
        {
            SetupAudioSession();
            SetupAudioUnit();
            SetupNotification();
        }

        public void AddSound(short[] sound)
        {
            _audioData.Buffer.Push(sound, true);
        }

        public void Start()
        {
            NSError error;
            if (!_audioSession.SetActive(true, AVAudioSessionFlags.NotifyOthersOnDeactivation, out error))
            {
                Log("Failed to start AudioSession");
            }

            AudioUnitStatus status = (AudioUnitStatus)_audioUnit.Initialize();
            if (status != AudioUnitStatus.NoError)
            {
                Log("Failed to Initialize audioUnit");
            }

            _audioUnit.Start();
        }

        private void Stop()
        {
            _audioUnit.Stop();
            _audioSession.SetActive(false);
        }

        public void Dispose()
        {
            _notification.Dispose();
        }

        private void SetupAudioSession()
        {
            _audioSession = AVAudioSession.SharedInstance();

            try
            {
                _audioSession.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.MixWithOthers);
                NSError error;
                if (!_audioSession.SetMode(AVAudioSession.ModeDefault, out error))
                    throw new Exception("ailed to setup AudioSession.SetMode");
            }
            catch (Exception e)
            {
                Log("Failed to setup AudioSession", e);
            }
        }

        private void SetupAudioUnit()
        {
            const int kAudioUnitSubType_RemoteIO = 1919512419;

            var defaultOutputDescription = new AudioComponentDescription
            {
                ComponentType = AudioComponentType.Output,
                ComponentSubType = kAudioUnitSubType_RemoteIO,
                ComponentManufacturer = AudioComponentManufacturerType.Apple,
                ComponentFlags = 0,
                ComponentFlagsMask = 0
            };

            var defaultOutput = AudioComponent.FindNextComponent(null, ref defaultOutputDescription);
            _audioUnit = defaultOutput.CreateAudioUnit();
            //_audioUnit = new AudioUnit.AudioUnit(defaultOutput);

            var audioStatus = _audioUnit.SetRenderCallback(RenderCallback, AudioUnitScopeType.Input, 0);

            if (audioStatus != AudioUnitStatus.NoError)
            {
                Log("Can not SetInputCallback");
                return;
            }

            var streamFormat = new AudioStreamBasicDescription()
            {
                Format = AudioFormatType.LinearPCM,
                FormatFlags = AudioFormatFlags.LinearPCMIsSignedInteger | AudioFormatFlags.LinearPCMIsPacked,
                SampleRate = 4000,
                BitsPerChannel = 16,
                ChannelsPerFrame = 1,
                FramesPerPacket = 1,
                BytesPerFrame = 2,
                BytesPerPacket = 2,
                Reserved = 0
            };

            audioStatus = _audioUnit.SetFormat(streamFormat, AudioUnitScopeType.Input, 0);
            if (audioStatus != AudioUnitStatus.NoError)
            {
                Log("Can not SetFormat");
                return;
            }
        }

        private void SetupNotification()
        {
            _notification = AVAudioSession.Notifications.ObserveInterruption(InterruptionCallback);
        }

        private AudioUnitStatus RenderCallback(AudioUnitRenderActionFlags flags, AudioTimeStamp inTimeStamp, uint inBusNumber, uint numberFrames, AudioBuffers data)
        {
            unsafe
            {
                var buffer = _audioData.Buffer.Pop((int)numberFrames);

                if (buffer == null)
                {
                    buffer = new short[numberFrames];
                }

                for (var i = 0; i < buffer.Length; ++i)
                    buffer[i] = (short)GainedValue(buffer[i]);

                byte[] byteArray = new byte[buffer.Length * 2];
                Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);

                fixed (byte* p = byteArray)
                {
                    IntPtr ptr = (IntPtr)p;
                    data.SetData(0, ptr);
                }
            }

            return AudioUnitStatus.NoError;
        }

        private void InterruptionCallback(object sender, AVAudioSessionInterruptionEventArgs args)
        {
            if (args.InterruptionType == AVAudioSessionInterruptionType.Began)
            {
                //  Начало прерывания воспроизвидения. Например, в случае, когда ктото позвонил
                Stop();
            }

            if (args.InterruptionType == AVAudioSessionInterruptionType.Ended)
            {
                //  Прерывание закончено. можно повторно воспроизводить звук
                Start();
            }
        }

        private int GainedValue(int val)
        {
            var gainedValue = val * _audioData.Amplifier;

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


        private void Log(string message, Exception exception = null)
        {
            Debug.WriteLine($"{message}, {exception?.Message}");
            Debug.WriteLine(exception?.StackTrace);
        }
    }
}
