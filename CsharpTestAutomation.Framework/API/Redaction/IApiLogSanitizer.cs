namespace CsharpTestAutomation.Framework.API.Redaction;

/// <summary>
/// Redacts sensitive values from API request/response data before they are logged or attached to reports.
/// Redaction is enabled by default via <see cref="Configuration.ApiLoggingSettings.RedactSensitiveData"/>.
/// When explicitly disabled, both methods return their input unchanged.
/// </summary>
public interface IApiLogSanitizer
{
    /// <summary>
    /// Returns a copy of <paramref name="headers"/> with sensitive values replaced by <c>***REDACTED***</c>
    /// when redaction is enabled; otherwise returns <paramref name="headers"/> unchanged.
    /// </summary>
    IReadOnlyDictionary<string, string> SanitizeHeaders(IReadOnlyDictionary<string, string> headers);

    /// <summary>
    /// Returns <paramref name="body"/> with sensitive JSON field values replaced by <c>***REDACTED***</c>
    /// when redaction is enabled; otherwise returns <paramref name="body"/> unchanged.
    /// Returns the original string unchanged if it is not valid JSON.
    /// </summary>
    string SanitizeBody(string body);
}
