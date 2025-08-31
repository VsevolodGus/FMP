using System;
using Bioss.Ultrasound.Data.Database.Entities.Enums;

namespace Bioss.Ultrasound.Domain.Models
{
    public class FhrEvent
    {
        public long Id { get; set; }
        public DateTime Time { get; set; }
        public Events Event { get; set; }
        public long RecordId { get; set; }
    }
}
