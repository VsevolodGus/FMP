using System;
using Bioss.Ultrasound.Tools;
using Xunit;

namespace Bioss.Ultrasound.Tests.Tools
{
    public class DateToolsTests
    {
        [Fact]
        public void CalculateAge_WhenBirthdayIsToday_ReturnsFullAge()
        {
            var now = new DateTime(2026, 3, 13);
            var birthDate = new DateTime(2006, 3, 13);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateAge_WhenBirthdayHasAlreadyPassedThisYear_ReturnsFullAge()
        {
            var now = new DateTime(2026, 3, 13);
            var birthDate = new DateTime(2006, 3, 12);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateAge_WhenBirthdayHasNotOccurredYetThisYear_ReturnsAgeMinusOne()
        {
            var now = new DateTime(2026, 3, 13);
            var birthDate = new DateTime(2006, 3, 14);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(19, result);
        }

        [Fact]
        public void CalculateAge_WhenBornToday_ReturnsZero()
        {
            var now = new DateTime(2026, 3, 13);
            var birthDate = new DateTime(2026, 3, 13);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateAge_WhenDateOfBirthIsInFuture_ReturnsNegativeAge()
        {
            var now = new DateTime(2026, 3, 13);
            var birthDate = new DateTime(2027, 3, 13);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(-1, result);
        }

        [Fact]
        public void CalculateAge_ForLeapDayBirth_BeforeMarchFirstInNonLeapYear_ReturnsAgeMinusOne()
        {
            var now = new DateTime(2025, 2, 28);
            var birthDate = new DateTime(2004, 2, 29);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateAge_ForLeapDayBirth_OnMarchFirstInNonLeapYear_ReturnsFullAge()
        {
            var now = new DateTime(2025, 3, 1);
            var birthDate = new DateTime(2004, 2, 29);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(21, result);
        }

        [Fact]
        public void CalculateAge_WhenNowIsNotPassed_UsesProvidedNowInsteadOfSystemClock()
        {
            var now = new DateTime(2030, 10, 5);
            var birthDate = new DateTime(2000, 10, 6);

            var result = birthDate.CalculateAge(now);

            Assert.Equal(29, result);
        }

        #region Тесты привязаны к реальному времени
        [Fact]
        public void CalculateAge_WhenBirthdayIsToday_ReturnsFullAge_RealTimeTest()
        {
            var now = DateTime.Now;
            var birthDate = now.AddYears(-20);

            var result = birthDate.CalculateAge();

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateAge_WhenBirthdayHasAlreadyPassedThisYear_ReturnsFullAge_RealTimeTest()
        {
            var now = DateTime.Now;
            var birthDate = now.AddYears(-20).AddDays(-1);

            var result = birthDate.CalculateAge();

            Assert.Equal(20, result);
        }

        [Fact]
        public void CalculateAge_WhenBirthdayHasNotOccurredYetThisYear_ReturnsAgeMinusOne_RealTimeTest()
        {
            var now = DateTime.Now;
            var birthDate = now.AddYears(-20).AddDays(1);

            var result = birthDate.CalculateAge();

            Assert.Equal(19, result);
        }

        [Fact]
        public void CalculateAge_WhenBornToday_ReturnsZero_RealTimeTest()
        {
            var birthDate = DateTime.Now;

            var result = birthDate.CalculateAge();

            Assert.Equal(0, result);
        }

        [Fact]
        public void CalculateAge_WhenDateOfBirthIsInFuture_ReturnsNegativeAge_RealTimeTest()
        {
            var birthDate = DateTime.Now.AddYears(1);

            var result = birthDate.CalculateAge();

            Assert.Equal(-1, result);
        }
    }
    #endregion
}