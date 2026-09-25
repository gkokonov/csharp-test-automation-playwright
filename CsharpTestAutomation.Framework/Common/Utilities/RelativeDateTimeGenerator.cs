using System.Globalization;
using System.Text.RegularExpressions;

namespace Csharp.Core.Testframework.Common.Utilities;

/// <summary>
/// Utility class for parsing relative date and time strings.
/// </summary>
public static partial class RelativeDateTimeGenerator
{
    private const string DefaultDateFormat = "dd/MM/yyyy";
    private const string DefaultTimeFormat = "HH:mm:ss";

    private static readonly Regex s_datePattern = DatePatternRegex();
    private static readonly Regex s_timePattern = TimePatternRegex();

    /// <summary>
    /// Parses the input string and returns a formatted date or time string, depending on the pattern matched.
    /// </summary>
    /// <param name="input">The input string describing a relative date or time (e.g., "1 day before today", "2 hours after now").</param>
    /// <returns>A formatted date ("dd/MM/yyyy") or time ("HH:mm:ss") string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if input is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown if input does not match any supported pattern.</exception>
    public static string GenerateRelativeDataTime(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        if (s_datePattern.IsMatch(input.Trim()))
        {
            return GenerateRelativeDate(input);
        }
        else if (s_timePattern.IsMatch(input.Trim()))
        {
            return GenerateRelativeGetTime(input);
        }
        else
        {
            throw new FormatException($"Input does not match date or time pattern: '{input}'");
        }
    }

    /// <summary>
    /// Parses the input string and returns a formatted date or time string using the specified format.
    /// </summary>
    /// <param name="input">The input string describing a relative date or time.</param>
    /// <param name="format">The format string for the output (e.g., "dd/MM/yyyy" or "HH:mm:ss").</param>
    /// <returns>A formatted date or time string.</returns>
    /// <exception cref="ArgumentNullException">Thrown if input is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown if input does not match any supported pattern.</exception>
    public static string GenerateRelativeDataTime(string input, string format)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        if (s_datePattern.IsMatch(input.Trim()))
        {
            return GenerateRelativeDate(input, format);
        }
        else if (s_timePattern.IsMatch(input.Trim()))
        {
            return GenerateRelativeGetTime(input, format);
        }
        else
        {
            throw new FormatException($"Input does not match date or time pattern: '{input}'");
        }
    }

    /// <summary>
    /// Parses a relative date string and returns the corresponding <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="input">The input string describing a relative date (e.g., "1 day before today").</param>
    /// <param name="format">The format string for the output (default is "dd/MM/yyyy").</param>
    /// <returns>A <see cref="DateTime"/> representing the calculated date.</returns>
    /// <exception cref="ArgumentNullException">Thrown if input is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown if input does not match the date pattern.</exception>
    public static DateTime ParseRelativeDate(string input, string format = DefaultDateFormat)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        Match match = s_datePattern.Match(input.Trim());
        if (!match.Success)
            throw new FormatException($"Invalid date string: '{input}'");

        var value = int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        var direction = match.Groups["direction"].Value.ToLowerInvariant();

        DateTime baseDate = DateTime.Today;
        var sign = direction == "before" ? -1 : 1;

        DateTime result = unit switch {
            "day" or "days" => baseDate.AddDays(sign * value),
            "month" or "months" => baseDate.AddMonths(sign * value),
            "year" or "years" => baseDate.AddYears(sign * value),
            _ => throw new FormatException($"Unsupported unit: '{unit}'")
        };

        return result.Date;
    }

    /// <summary>
    /// Parses a relative date string and returns the formatted date string.
    /// </summary>
    /// <param name="input">The input string describing a relative date.</param>
    /// <param name="format">The format string for the output (default is "dd/MM/yyyy").</param>
    /// <returns>A formatted date string.</returns>
    public static string GenerateRelativeDate(string input, string format = DefaultDateFormat)
    {
        return ParseRelativeDate(input, format).ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses a relative time string and returns the corresponding <see cref="DateTime"/> value.
    /// </summary>
    /// <param name="input">The input string describing a relative time (e.g., "2 hours before now").</param>
    /// <param name="format">The format string for the output (default is "HH:mm:ss").</param>
    /// <returns>A <see cref="DateTime"/> representing the calculated time.</returns>
    /// <exception cref="ArgumentNullException">Thrown if input is null or whitespace.</exception>
    /// <exception cref="FormatException">Thrown if input does not match the time pattern.</exception>
    public static DateTime ParseRelativeGetTime(string input, string format = DefaultTimeFormat)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentNullException(nameof(input));

        Match match = s_timePattern.Match(input.Trim());
        if (!match.Success)
            throw new FormatException($"Invalid time string: '{input}'");

        var value = int.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups["unit"].Value.ToLowerInvariant();
        var direction = match.Groups["direction"].Value.ToLowerInvariant();

        DateTime baseTime = DateTime.Now;
        var sign = direction == "before" ? -1 : 1;

        DateTime result = unit switch {
            "hour" or "hours" => baseTime.AddHours(sign * value),
            "min" or "minute" or "minutes" => baseTime.AddMinutes(sign * value),
            "sec" or "second" or "seconds" => baseTime.AddSeconds(sign * value),
            _ => throw new FormatException($"Unsupported time unit: '{unit}'")
        };

        return result;
    }

    /// <summary>
    /// Parses a relative time string and returns the formatted time string.
    /// </summary>
    /// <param name="input">The input string describing a relative time.</param>
    /// <param name="format">The format string for the output (default is "HH:mm:ss").</param>
    /// <returns>A formatted time string.</returns>
    public static string GenerateRelativeGetTime(string input, string format = DefaultTimeFormat)
    {
        return ParseRelativeGetTime(input, format).ToString(format, CultureInfo.InvariantCulture);
    }

    [GeneratedRegex(@"(?<value>\d+)\s*(?<unit>day|month|year)s?\s*(?<direction>before|after)\s*(?<reference>today)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex DatePatternRegex();

    [GeneratedRegex(@"(?<value>\d+)\s*(?<unit>hour|min|minute|sec|second|)s?\s*(?<direction>before|after)\s*(?<reference>now)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex TimePatternRegex();
}
