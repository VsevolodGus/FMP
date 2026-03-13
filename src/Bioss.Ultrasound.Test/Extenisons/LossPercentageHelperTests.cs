using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Bioss.Ultrasound.Domain.Collections;
using Bioss.Ultrasound.UI.Helpers;
using Xunit;

namespace Bioss.Ultrasound.Tests.UI.Helpers
{
    public class LossPercentageHelperTests
    {
        [Fact]
        public void NewHelper_HasEmptyState()
        {
            var helper = new LossPercentageHelper();

            Assert.Equal(0d, helper.PercentAll());
            Assert.Equal(0d, helper.PercentInMin());
            Assert.False(helper.IsQueryFull);
            Assert.False(helper.IsError);
        }

        [Fact]
        public void Add_CalculatesPercentAll_AndPercentInMin()
        {
            var helper = new LossPercentageHelper();

            helper.Add(0);
            helper.Add(10);
            helper.Add(0);
            helper.Add(20);

            Assert.Equal(0.5d, helper.PercentAll());
            Assert.Equal(0.5d, helper.PercentInMin(), 6);
            Assert.False(helper.IsQueryFull);
            Assert.False(helper.IsError);
        }

        [Fact]
        public void IsError_IsFalse_WhileQueueIsNotFull()
        {
            var helper = new LossPercentageHelper();

            helper.Add(0);
            helper.Add(0);
            helper.Add(1);

            Assert.False(helper.IsQueryFull);
            Assert.Equal(2d / 3d, helper.PercentInMin(), 6);
            Assert.False(helper.IsError);
        }

        [Fact]
        public void IsError_BecomesTrue_WhenQueueIsFull_AndErrorPercentIsGreaterThanThreshold()
        {
            var helper = new LossPercentageHelper();
            SeedExpiredValue(helper, 1);

            helper.Add(0);
            helper.Add(0);
            helper.Add(1);

            Assert.True(helper.IsQueryFull);
            Assert.Equal(2d / 3d, helper.PercentInMin(), 6);
            Assert.True(helper.IsError);
        }

        [Fact]
        public void IsError_IsFalse_WhenErrorPercentEqualsThreshold()
        {
            var helper = new LossPercentageHelper();
            SeedExpiredValue(helper, 1);

            helper.Add(0);
            helper.Add(1);
            helper.Add(1);
            helper.Add(1);

            Assert.True(helper.IsQueryFull);
            Assert.Equal(0.25d, helper.PercentInMin(), 6);
            Assert.False(helper.IsError);
        }

        [Fact]
        public void Clear_ResetsAllState()
        {
            var helper = new LossPercentageHelper();
            SeedExpiredValue(helper, 1);

            helper.Add(0);
            helper.Add(1);

            Assert.True(helper.IsQueryFull);
            Assert.True(helper.PercentAll() > 0);

            helper.Clear();

            Assert.Equal(0d, helper.PercentAll());
            Assert.Equal(0d, helper.PercentInMin());
            Assert.False(helper.IsQueryFull);
            Assert.False(helper.IsError);
        }

        private static void SeedExpiredValue(LossPercentageHelper helper, byte value)
        {
            var queueField = typeof(LossPercentageHelper)
                .GetField("_queue", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(queueField);

            var timeQueue = Assert.IsType<TimeQueue<byte>>(queueField!.GetValue(helper));

            var innerQueueField = typeof(TimeQueue<byte>)
                .GetField("_queue", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(innerQueueField);

            var innerQueue =
                Assert.IsType<ConcurrentQueue<KeyValuePair<DateTime, byte>>>(innerQueueField!.GetValue(timeQueue));

            innerQueue.Enqueue(new KeyValuePair<DateTime, byte>(DateTime.Now.AddMinutes(-2), value));
        }
    }
}