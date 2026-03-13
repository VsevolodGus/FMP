using System;
namespace Bioss.Ultrasound.Tools
{
    public static class DateTools
    {
        public static int CalculateAge(this DateTime dateOfBirth, DateTime? now = null)
        {
            var currentDate = now ?? DateTime.Now;

            var age = currentDate.Year - dateOfBirth.Year;
            if (currentDate.DayOfYear < dateOfBirth.DayOfYear)
                age--;

            return age;
        }
    }
}
