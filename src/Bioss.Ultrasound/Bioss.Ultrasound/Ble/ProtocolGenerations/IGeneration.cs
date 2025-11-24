using System;
namespace Bioss.Ultrasound.Ble.ProtocolGenerations
{
    public interface IGeneration
    {
        Guid SrCustom { get; }
        Guid ChCustomRead { get; }
        Guid ChCustomWrite { get; }
    }
}

