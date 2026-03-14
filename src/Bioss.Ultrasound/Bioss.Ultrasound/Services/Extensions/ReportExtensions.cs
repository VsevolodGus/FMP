namespace Bioss.Ultrasound.Services.Extensions
{
    public static class ReportExtensions
    {
        public static int CalculateCountPages(int totalMinutes, int minutesInPage)
            => ((totalMinutes - 1) / minutesInPage) + 1;
       
        public static double CalculateMinuteInPage(double sizeDisplayCentimeter, int centimetersInMinute)
            => sizeDisplayCentimeter / centimetersInMinute;
    }
}
