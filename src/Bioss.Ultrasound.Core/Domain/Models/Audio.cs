namespace Bioss.Ultrasound.Core.Domain.Models;

public class Audio
{
    public long Id { get; set; }
    public List<short> Sound { get; set; } = new List<short>();
    public long RecordId { get; set; }
}
