namespace Bioss.Ultrasound.Core.Tools;

public static class StringTools
{
    public static string ToStringOrEmptyString(this double value, string emptyString = "", string format = null)
    {
        if (value == 0)
            return emptyString;


        return string.IsNullOrEmpty(format)
            ? value.ToString()
            : value.ToString(format);
    }
    public static string ToStringOrEmptyString(this int value, string emptyString = "", string format = null)
    {
        if (value == 0)
            return emptyString;


        return string.IsNullOrEmpty(format)
            ? value.ToString()
            : value.ToString(format);
    }
}
