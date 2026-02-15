using Bioss.Ultrasound.Core.Ble.ProtocolGenerations;
using Plugin.BLE.Abstractions.Contracts;

namespace Bioss.Ultrasound.Core.Ble;

/// <summary>
/// Класс для управления нужными гуидами
/// Есть две версии устройства, старая (gen1) и новая (gen2). BLE протокол у них не отличается, но guid-ы отличаються
/// Этот класс помогает использовать правильное гуиды
/// </summary>
public class GuidsManager
{
    public static async Task<IGeneration> GetGeneration(IDevice device)
    {
        var services = await device.GetServicesAsync();

        foreach (var s in services)
        {
            var ch = await s.GetCharacteristicsAsync();
            foreach (var c in ch)
            {
                if (c.Id == Guids.CH_CUSTOM_READ_2)
                    return new Generation2();
            }
        }

        return new Generation1();
    }
}

