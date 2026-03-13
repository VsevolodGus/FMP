using Bioss.Ultrasound.Domain.Plotting;

namespace Bioss.Ultrasound.Test.Extenisons
{
    public class PlottingTimeSpanHelperTests
    {
        [Fact]
        public void CollectTimeSpan_AfterResetWithDate_ReturnsExactDifference()
        {
            var helper = new PlottingTimeSpanHelper();
            var start = new DateTime(2026, 3, 13, 10, 0, 0, DateTimeKind.Utc);
            var end = start.AddSeconds(42).AddMilliseconds(250);

            helper.Reset(start);

            var result = helper.CollectTimeSpan(end);

            Assert.Equal(TimeSpan.FromSeconds(42.25), result);
        }

        [Fact]
        public void Reset_WithDate_ChangesStartPointForNextCalculation()
        {
            var helper = new PlottingTimeSpanHelper();
            var firstStart = new DateTime(2026, 3, 13, 10, 0, 0);
            var secondStart = firstStart.AddSeconds(10);
            var target = firstStart.AddSeconds(15);

            helper.Reset(firstStart);
            var firstResult = helper.CollectTimeSpan(target);

            helper.Reset(secondStart);
            var secondResult = helper.CollectTimeSpan(target);

            Assert.Equal(TimeSpan.FromSeconds(15), firstResult);
            Assert.Equal(TimeSpan.FromSeconds(5), secondResult);
        }

        [Fact]
        public void CollectTimeSpan_CanReturnNegativeValue()
        {
            var helper = new PlottingTimeSpanHelper();
            var start = new DateTime(2026, 3, 13, 10, 0, 10);
            var earlier = new DateTime(2026, 3, 13, 10, 0, 0);

            helper.Reset(start);

            var result = helper.CollectTimeSpan(earlier);

            Assert.Equal(TimeSpan.FromSeconds(-10), result);
        }

        [Fact]
        public void Reset_WithoutDate_SetsStartTimeCloseToNow()
        {
            var helper = new PlottingTimeSpanHelper();

            helper.Reset();
            Thread.Sleep(30);

            var result = helper.CollectTimeSpan(DateTime.Now);

            Assert.InRange(result.TotalMilliseconds, 10, 500);
        }
    }
}