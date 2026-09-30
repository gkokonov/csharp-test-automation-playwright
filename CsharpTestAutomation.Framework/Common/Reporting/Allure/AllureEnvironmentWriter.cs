using System.Text;

namespace CsharpTestAutomation.Framework.Common.Reporting.Allure;

/// <summary>Writes an Allure <c>environment.properties</c> file to the results directory.</summary>
public static class AllureEnvironmentWriter
{
    private const string EnvironmentFileName = "environment.properties";

    /// <summary>
    /// Writes ordered key-value entries to <c>environment.properties</c>. The output directory is
    /// created when it does not exist and an existing file is overwritten.
    /// </summary>
    /// <param name="entries">Ordered key-value entries to write.</param>
    /// <param name="outputDirectory">Allure results directory; defaults to <c>allure-results</c>.</param>
    public static void Write(IEnumerable<KeyValuePair<string, string>> entries, string outputDirectory = "allure-results")
    {
        ArgumentNullException.ThrowIfNull(entries);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);
        var builder = new StringBuilder();
        foreach (KeyValuePair<string, string> entry in entries)
        {
            builder.AppendLine(string.IsNullOrEmpty(entry.Key) ? string.Empty : $"{entry.Key}={entry.Value}");
        }

        File.WriteAllText(Path.Combine(outputDirectory, EnvironmentFileName), builder.ToString(), Encoding.UTF8);
    }
}
