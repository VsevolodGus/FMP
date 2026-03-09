using System;

namespace Bioss.Ultrasound.Ble.Models
{
    public class Package
    {
        /// <summary>
        /// Дата создания пакета
        /// </summary>
        public DateTime ReceivedAt { get; private set; }

        /// <summary>
        /// Sound package
        /// Sending interval: 50ms
        /// </summary>
        public SoundPackage SoundPackage { get; private set; }

        /// <summary>
        /// FHR package
        /// Sending interval: 250ms
        /// </summary>
        public FHRPackage FHRPackage { get; private set; }

        public bool IsValid
        {
            get
            {
                if (SoundPackage != null && FHRPackage != null)
                    return SoundPackage.IsValid && FHRPackage.IsValid;

                return SoundPackage?.IsValid ?? false;
            }
        }

        public static Package Init(byte[] data)
        {
            var size = data.Length;

            var package = new Package();

            if (size == SoundPackage.DataLength + FHRPackage.DataLength)
            {
                var soundsData = new byte[SoundPackage.DataLength];
                var fhrData = new byte[FHRPackage.DataLength];

                Array.Copy(data, soundsData, SoundPackage.DataLength);
                Array.Copy(data, SoundPackage.DataLength, fhrData, 0, FHRPackage.DataLength);

                package.SoundPackage = SoundPackage.Init(soundsData);
                package.FHRPackage = FHRPackage.Init(fhrData);
                package.ReceivedAt = DateTime.Now;
                return package;
            }
            else if (size == SoundPackage.DataLength)
            {
                var soundsData = new byte[SoundPackage.DataLength];
                Array.Copy(data, soundsData, SoundPackage.DataLength);
                package.SoundPackage = SoundPackage.Init(soundsData);
                package.ReceivedAt = DateTime.Now;
                return package;
            }

            return null;
        }

        public static Package Init(in ReadOnlySpan<byte> data)
        {
            var size = data.Length;

            var package = new Package();

            if (size == SoundPackage.DataLength + FHRPackage.DataLength)
            {
                // Используем Slice вместо Array.Copy
                var soundsSpan = data.Slice(0, SoundPackage.DataLength);
                var fhrSpan = data.Slice(SoundPackage.DataLength, FHRPackage.DataLength);

                package.SoundPackage = SoundPackage.Init(soundsSpan);
                package.FHRPackage = FHRPackage.Init(fhrSpan);
                package.ReceivedAt = DateTime.Now;

                return package;
            }
            else if (size == SoundPackage.DataLength)
            {
                var soundsSpan = data.Slice(0, SoundPackage.DataLength);
                
                package.SoundPackage = SoundPackage.Init(soundsSpan);
                package.ReceivedAt = DateTime.Now;

                return package;
            }

            return null;
        }
    }
}
