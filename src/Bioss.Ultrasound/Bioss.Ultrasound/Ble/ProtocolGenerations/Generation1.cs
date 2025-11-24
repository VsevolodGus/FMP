using System;
namespace Bioss.Ultrasound.Ble.ProtocolGenerations
{
    public class Generation1 : IGeneration
    {
        public Guid SrCustom => Guids.SR_CUSTOM;
        public Guid ChCustomRead => Guids.CH_CUSTOM_READ;
        public Guid ChCustomWrite => Guids.CH_CUSTOM_WRITE;
    }
}

