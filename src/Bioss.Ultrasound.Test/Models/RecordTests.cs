using Bioss.Ultrasound.Domain.Models;
using Record = Bioss.Ultrasound.Domain.Models.Record;


namespace Bioss.Ultrasound.Tests.Domain.Models
{
    public class RecordTests
    {
        [Fact]
        public void NewRecord_InitializesCollectionsAndAudio()
        {
            var record = new Record();

            Assert.NotNull(record.Events);
            Assert.Empty(record.Events);

            Assert.NotNull(record.Fhrs);
            Assert.Empty(record.Fhrs);

            Assert.NotNull(record.Audio);
        }

        [Fact]
        public void RecordingTime_ReturnsFormattedMinutesAndSeconds()
        {
            var start = new DateTime(2026, 3, 13, 10, 0, 0);
            var stop = start.AddMinutes(2).AddSeconds(5);

            var record = new Record
            {
                StartTime = start,
                StopTime = stop
            };

            Assert.Equal("02:05", record.RecordingTime);
        }

        [Fact]
        public void RecordingTimeSpan_ReturnsDifferenceBetweenStopAndStart()
        {
            var start = new DateTime(2026, 3, 13, 10, 0, 0);
            var stop = start.AddMinutes(3).AddSeconds(15);

            var record = new Record
            {
                StartTime = start,
                StopTime = stop
            };

            Assert.Equal(TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(15)), record.RecordingTimeSpan);
        }

        [Fact]
        public void LossPercentage_WhenNoFhrs_ReturnsZero()
        {
            var record = new Record();

            Assert.Equal(0d, record.LossPercentage);
        }

        [Fact]
        public void LossPercentage_ReturnsCorrectRatio()
        {
            var record = new Record
            {
                Fhrs =
                {
                    CreateFhrData(0),
                    CreateFhrData(120),
                    CreateFhrData(0),
                    CreateFhrData(130)
                }
            };

            Assert.Equal(0.5d, record.LossPercentage);
        }

        private static FhrData CreateFhrData(object fhrValue)
        {
            var item = (FhrData?)Activator.CreateInstance(typeof(FhrData), nonPublic: true);
            Assert.NotNull(item);

            var prop = typeof(FhrData).GetProperty("Fhr");
            Assert.NotNull(prop);

            var converted = Convert.ChangeType(fhrValue, prop!.PropertyType);
            prop.SetValue(item, converted);

            return item!;
        }
    }
}