using System;
namespace Bioss.Ultrasound.Tools
{
    public class DateTools
    {
        public static int CalculateAge(DateTime dateOfBirth)
        {
            var age = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.DayOfYear < dateOfBirth.DayOfYear)
                age = age - 1;

            return age;
        }

        public static (int weeks, int days) CalculatePregnantTime(DateTime pregnantStart)
        {
            return CalculatePregnantTime(pregnantStart, DateTime.Now);
        }

        public static (int weeks, int days) CalculatePregnantTime(DateTime pregnantStart, DateTime toDate)
        {
            var pregnantDays = (toDate - pregnantStart).Days;

            var weeksCount = pregnantDays / 7;
            var daysCount = pregnantDays - (weeksCount * 7);

            return (weeksCount, daysCount);
        }
    }
}
