using System.Collections.Generic;

namespace Bioss.Ultrasound.Domain.Models
{
    public class Audio
    {
        public long Id { get; set; }
        public List<short> Sound { get; set; } = new List<short>();
        public long RecordId { get; set; }
    }
}
