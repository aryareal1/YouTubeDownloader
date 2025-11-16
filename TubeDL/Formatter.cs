namespace TubeDL;

public class Formatter
{
    /// <summary>
    /// Formats a number into a metric representation (e.g., 1.5k, 2.3M).
    /// </summary>
    /// <param name="value">The value to format</param>
    /// <param name="fractionDigits">How many decimal point to include</param>
    /// <param name="separator">A string to separate between the value and the types ("2.5{separator}M")</param>
    /// <param name="divider">The amount to divide the value</param>
    /// <param name="types">Type for the format</param>
    /// <returns>A string with number formatted with the types</returns>
    public static string MetricNumber(
        double value,
        int fractionDigits = 1,
        string separator = "",
        double divider = 1000,
        string[]? types = null
    )
    {
        types ??= ["", "k", "M", "G", "T", "P", "E", "Z", "Y"];

        if (value == 0)
            return "0";

        int index = (int)Math.Floor(Math.Log(Math.Abs(value), divider));
        index = Math.Clamp(index, 0, types.Length - 1);

        double scaled = value / Math.Pow(divider, index);
        return $"{scaled.ToString($"F{fractionDigits}")}{separator}{types[index]}";
    }

    /// <summary>
    /// Convert milliseconds into a human readeble relative duration
    /// </summary>
    /// <param name="milliseconds">Duration long in milliseconds</param>
    public static string RelativeDuration(long milliseconds)
    {
        if (milliseconds < 1000)
            return $"{milliseconds} milidetik";

        long seconds = milliseconds / 1000;
        if (seconds < 60)
            return $"{seconds} detik";

        long minutes = seconds / 60;
        if (minutes < 60)
            return $"{minutes} menit";

        long hours = minutes / 60;
        if (hours < 24)
            return $"{hours} jam";

        long days = hours / 24;
        if (days < 30)
            return $"{days} hari";

        long months = days / 30;
        if (months < 12)
            return $"{months} bulan";

        long years = months / 12;
        return $"{years} tahun";
    }
}