using System;

namespace Bioss.Ultrasound.Ble.Models
{
    public class Package
    {
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
                return package;
            }
            else if (size == SoundPackage.DataLength)
            {
                var soundsData = new byte[SoundPackage.DataLength];
                Array.Copy(data, soundsData, SoundPackage.DataLength);
                package.SoundPackage = SoundPackage.Init(soundsData);
                return package;
            }

            return null;
        }
    }
}
