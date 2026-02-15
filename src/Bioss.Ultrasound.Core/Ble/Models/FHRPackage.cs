namespace Bioss.Ultrasound.Core.Ble.Models;

/// <summary>
///|  Name      |  Package Length  |  Note                   |
///|------------|------------------|-------------------------|
///|Package Head|  2byte           | 0x55 0xaa               |
///|Control Text|  1byte           | 0x03                    |
///|Data        |  N byte          | e.g.data 1 … data N   |
///|Parity Sum  |  1byte           |(data 1 + … +  dataN)%256|
/// </summary>
public class FHRPackage
{
    public const int DataLength = 10;

    // Package Head: 0x55
    public byte Head1 { get; private set; }

    // Package Head: 0xaa
    public byte Head2 { get; private set; }

    // Control Text: 0x03
    public byte Control { get; private set; }

    // Range: 50-210; if no, =0
    public byte Fhr1 { get; private set; }

    // Range: 50-210; if no, =0
    public byte Fhr2 { get; private set; }

    // Range: 0-100
    public byte Toco { get; private set; }

    // Reserved
    public byte Afm { get; private set; }

    // See `Status1Options` for details
    public QualityFhrSignalStatus QualitySignal { get; private set; }

    // See `Status2Options` for details
    public BattaryStatus Status2 { get; private set; }

    // (data 1 + … +  dataN) % 256
    public byte Parity { get; private set; }

    public bool IsValid
    {
        get
        {
            if (Head1 != 0x55)
                return false;

            if (Head2 != 0xaa)
                return false;

            if (Control != 0x03)
                return false;

            var sum = (Fhr1 + Fhr2 + Toco + Afm + QualitySignal.RawValue + Status2.RawValue) % 256;

            byte csc = sum < 256
                ? (byte)sum
                : (byte)0;

            if (csc != Parity)
                return false;

            return true;
        }
    }

    public static FHRPackage Init(byte[] data)
    {
        //guard data.count == FHRPackage.size else { return nil }
        //data.withUnsafeBytes { rawBufferPointer in
        //    let rawPtr = rawBufferPointer.baseAddress!
        //    self = rawPtr.load(fromByteOffset: 0, as: FHRPackage.self)
        //}

        if (data.Length != DataLength)
            return null;

        var package = new FHRPackage();
        package.Head1 = data[0];
        package.Head2 = data[1];
        package.Control = data[2];
        package.Fhr1 = data[3];
        package.Fhr2 = data[4];
        package.Toco = data[5];
        package.Afm = data[6];
        package.QualitySignal = new QualityFhrSignalStatus(data[7]);
        package.Status2 = new BattaryStatus(data[8]);
        package.Parity = data[9];

        return package;
    }

    public static FHRPackage Init(in ReadOnlySpan<byte> data)
    {
        if (data.Length != DataLength)
            return null;

        var package = new FHRPackage
        {
            Head1 = data[0],
            Head2 = data[1],
            Control = data[2],
            Fhr1 = data[3],
            Fhr2 = data[4],
            Toco = data[5],
            Afm = data[6],
            QualitySignal = new QualityFhrSignalStatus(data[7]),
            Status2 = new BattaryStatus(data[8]),
            Parity = data[9]
        };

        return package;
    }
}
