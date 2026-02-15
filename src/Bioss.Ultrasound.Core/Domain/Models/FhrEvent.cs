using Bioss.Ultrasound.Core.Data.Database.Entities.Enums;

namespace Bioss.Ultrasound.Core.Domain.Models;

public class FhrEvent
{
    public long Id { get; set; }
    public DateTime Time { get; set; }
    public Events Event { get; set; }
    public long RecordId { get; set; }
}
