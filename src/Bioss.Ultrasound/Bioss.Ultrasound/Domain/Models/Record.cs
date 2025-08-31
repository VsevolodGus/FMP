using System;
using System.Collections.Generic;
using System.Linq;

namespace Bioss.Ultrasound.Domain.Models
{
    public class Record
    {
        public long Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string DeviceSerialNumber { get; set; }
        public List<FhrEvent> Events { get; set; } = new List<FhrEvent>();
        public List<FhrData> Fhrs { get; set; } = new List<FhrData>();
        public Audio Audio { get; set; } = new Audio();
        public Biometric Biometric { get; set; }

        public string RecordingTime
        {
            get
            {
                var timeLeft = (StopTime - StartTime).TotalSeconds;
                var t = TimeSpan.FromSeconds(timeLeft);
                return $"{t.Minutes:D2}:{t.Seconds:D2}";
            }
        }

        public TimeSpan RecordingTimeSpan => (StopTime - StartTime);

        public double LossPercentage
        {
            get
            {
                if (!Fhrs.Any())
                    return .0;

                var lossCount = Fhrs.Count(a => a.Fhr == 0);
                return (double)lossCount / Fhrs.Count;
            }
        }
    }
}
