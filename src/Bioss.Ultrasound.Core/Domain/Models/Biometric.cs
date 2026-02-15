namespace Bioss.Ultrasound.Core.Domain.Models;

public class Biometric
{
    public long Id { get; set; }
    public long RecordId { get; set; }
    public string Comment { get; set; }
    public double Temperature { get; set; }
    public double Sugar { get; set; }
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public int Pulse { get; set; }
}
