using System.Globalization;
using NLog;

namespace CsharpTestAutomation.Framework.Common.Extensions;

public static class DateTimeExtensions
{
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    public static DateTime TryParseDate(string value, string fieldName, string format = "MM/dd/yyyy")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            s_log.Warn($"Warning: {fieldName} value is empty or null. Using default date.");
            return new DateTime(1, 1, 1);
        }

        if (DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture,
            DateTimeStyles.None, out DateTime result))
        {
            return result;
        }
        else
        {
            s_log.Warn($"Warning: Could not parse {fieldName} date '{value}'. Using default date.");
            return new DateTime(1, 1, 1);
        }
    }
}
