namespace Bioss.Ultrasound.Tools
{
    public class StringTools
    {
        public static string ToStringOrEmptyString(double value, string emptyString = "")
        {
            return value == .0
                ? emptyString
                : $"{value}";
        }
    }
}
