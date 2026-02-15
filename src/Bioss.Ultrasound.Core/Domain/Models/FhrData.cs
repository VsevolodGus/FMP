namespace Bioss.Ultrasound.Core.Domain.Models;

public class FhrData
{
    public long Id { get; set; }
    public long RecordId { get; set; }
    public DateTime Time { get; set; }
    public byte Fhr { get; set; }
    public byte Toco { get; set; }
}
