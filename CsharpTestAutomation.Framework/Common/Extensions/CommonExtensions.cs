namespace CsharpTestAutomation.Framework.Common.Extensions;

/// <summary>
/// Simple helper class to be used for most general helpers
/// </summary>
public static class CommonExtensions
{
    /// <summary>
    /// Return unique 18 characters number.
    /// </summary>
    public static long UtcNowTicks {
        get {
            long original, newValue;
            do
            {
                original = field;
                var now = DateTime.UtcNow.Ticks;
                newValue = Math.Max(now, original + 1);
            } while (Interlocked.CompareExchange
                         (ref field, newValue, original) != original);

            return newValue;
        }
    } = DateTime.UtcNow.Ticks;

    /// <summary>
    /// Convenient method to get the string value of a variable or alternative if the target is
    /// null or empty
    /// </summary>
    /// <param name="target">the string that is to be expected for null or empty</param>
    /// <param name="alternative">
    /// the alternative string that is to be return if "target" is null or empty. Note! no check
    /// for null or empty are applied to the alternative:
    /// </param>
    /// <returns>
    /// the original value of the "target" parameter if it is not null or empty, otherwise
    /// returns the original value of the "alternative" parameter.
    /// </returns>
    public static string GetNonEmptyValueOrAlternative(string target, string alternative) => string.IsNullOrEmpty(target) ? alternative : target;

    /// <summary>
    /// Generates Unix Timestamp in milliseconds from DateTime.UtcNow.
    /// </summary>
    /// <returns>Unix Timestamp as string</returns>
    public static string GetCurrentUnixTimestamp() => new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
}
