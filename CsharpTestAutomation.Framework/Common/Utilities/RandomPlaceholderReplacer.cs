using System.Text.RegularExpressions;
using Bogus;

namespace Csharp.Core.Testframework.Common.Utilities;

/// <summary>
/// Utility class for replacing placeholders in a string with random characters.
/// </summary>
public static partial class RandomPlaceholderReplacer
{
    private static readonly Randomizer s_random = new();

    private static readonly Regex s_placeholderPattern = PlaceHolderReg();

    /// <summary>
    /// Replaces placeholders in the input string with random characters.
    /// [dN] - N random digits, [aN] - N random lowercase letters, [AN] - N random uppercase letters.
    /// </summary>
    /// <param name="input">Input string containing placeholders.</param>
    /// <returns>String with placeholders replaced by random characters.</returns>
    public static string RandomizeValue(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        return s_placeholderPattern.Replace(input, match =>
        {
            var type = match.Groups[1].Value;
            var count = int.Parse(match.Groups[2].Value);

            return type switch {
                "d" => s_random.String(count, '0', '9'),
                "a" => s_random.String(count, 'a', 'z'),
                "A" => s_random.String(count, 'A', 'Z'),
                _ => match.Value
            };
        });
    }

    [GeneratedRegex(@"\[(d|a|A)(\d+)\]", RegexOptions.Compiled)]
    private static partial Regex PlaceHolderReg();
}
