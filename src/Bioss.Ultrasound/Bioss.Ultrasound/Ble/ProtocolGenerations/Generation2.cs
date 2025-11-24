using System;
namespace Bioss.Ultrasound.Ble.ProtocolGenerations
{
    public class Generation2 : IGeneration
    {
        public Guid SrCustom => Guids.SR_CUSTOM_2;
        public Guid ChCustomRead => Guids.CH_CUSTOM_READ_2;
        public Guid ChCustomWrite => Guids.CH_CUSTOM_WRITE_2;
    }
}

