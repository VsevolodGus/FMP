using Bioss.Ultrasound.Core.IO;

namespace Bioss.Ultrasound.Core.Ble.Models;

/**
|  Name      |  Package Length  |  Note                   |
|------------|------------------|-------------------------|
|Package Head|  2byte           | 0x55 0xaa               |
|Control Text|  1byte           | 0x09                    |
|Data        |  N byte          | e.g.  data 1 … data N   |
|Parity Sum  |  1byte           |(data 1 + … +  dataN)%256|


This data include 107bytes,  the last 3 bytes are extended bytes ( from Version 2.0) and it is used for
transmitting 2 Data Vairants of ADPCM coding. Thus it can help to decode sound data correctly.

 ADPCM Index  & ADPCM Valpred:  2 coding variants before coding each sound data package.

Receiver capture “ADPCM Index” from package, then value to “ADPCM Index”, then merge 8 high digits
and 8 low digits of “ Valpred” into one “int”, then value to “ADPCM Valpred”, then decoding.

 # Remark

 During data communication, there may be data package loss or non-simultaneous data package, thus receiver may not decode sound data correctly. So we extend 3bytes.
*/
public class SoundPackage
{
    public const int SoundSize = 100;
    public static int DataLength => SoundSize + 7;

    /// Package Head: 0x55
    public byte Head1 { get; private set; }
    /// Package Head: 0xaa
    public byte Head2 { get; private set; }
    /// Control Text: 0x09
    public byte Control { get; private set; }

    /// Data communication coding system is ADPCM. Thus, at receiver end, 
    /// 100pcs of data are decoded into 200pcs of data. 
    /// These 200pcs of data can be sent to sound playing directly.
    public byte[] Data { get; private set; }

    ///Parity Sum occupies the  104th byte, it is Parity Sum of S1-S100
    ///Not used in this version of protocol*
    public byte Parity { get; private set; }
    public byte AdpcmIndex { get; private set; }
    public byte AdpcmValueLow { get; private set; }
    public byte AdpcmValueHigh { get; private set; }

    public bool IsValid
    {
        get
        {
            if (Head1 != 0x55)
                return false;

            if (Head2 != 0xaa)
                return false;

            if (Control != 0x09)
                return false;

            // parity check is not used
            return true;
        }
    }

    public short[] Decompress()
    {
        int stepIndex = AdpcmIndex;
        int predictor = AdpcmValueLow | AdpcmValueHigh << 8;

        var prev = new Adpcm.ImaAdpcmState
        {
            Index = stepIndex,
            ValPrev = predictor
        };
        var len = Data.Length * 2;
        return Adpcm.Decode(Data, len, prev);
    }

    public static SoundPackage Init(byte[] data)
    {
        if (data.Length != DataLength)
            return null;

        var package = new SoundPackage();
        package.Head1 = data[0];
        package.Head2 = data[1];
        package.Control = data[2];

        var soundArray = new byte[SoundSize];
        Array.Copy(data, 3, soundArray, 0, SoundSize);
        package.Data = soundArray;

        package.Parity = data[103];
        package.AdpcmIndex = data[104];
        package.AdpcmValueLow = data[105];
        package.AdpcmValueHigh = data[106];

        return package;
    }

    public static SoundPackage Init(in ReadOnlySpan<byte> data)
    {
        if (data.Length != DataLength)
            return null;

        var package = new SoundPackage
        {
            Head1 = data[0],
            Head2 = data[1],
            Control = data[2],
            Parity = data[103],
            AdpcmIndex = data[104],
            AdpcmValueLow = data[105],
            AdpcmValueHigh = data[106]
        };

        // Копируем данные звука
        var soundArray = new byte[SoundSize];
        data.Slice(3, SoundSize).CopyTo(soundArray);
        package.Data = soundArray;

        return package;
    }

}
