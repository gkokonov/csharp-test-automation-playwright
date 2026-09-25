namespace CsharpTestAutomation.Framework.API.Logging;

/// <summary>
/// Immutable, behaviour-free snapshot of a single API call used for logging and report attachments.
/// String values are redacted by the caller only when redaction is enabled; otherwise they are raw.
/// </summary>
public record ApiLogEntry
{
    /// <summary>HTTP method used for the request.</summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>Relative resource path that was requested.</summary>
    public string Resource { get; init; } = string.Empty;

    /// <summary>Absolute URI that responded, when available.</summary>
    public Uri? FullUri { get; init; } // default null if not set

    /// <summary>Request headers, redacted only when redaction is enabled.</summary>
    public IReadOnlyDictionary<string, string> RequestHeaders { get; init; } = new Dictionary<string, string>();

    /// <summary>Request body, redacted only when redaction is enabled.</summary>
    public string RequestBody { get; init; } = string.Empty;

    /// <summary>HTTP status code returned by the server.</summary>
    public int StatusCode { get; init; }

    /// <summary>HTTP status reason phrase, when available.</summary>
    public string ReasonPhrase { get; init; } = string.Empty;

    /// <summary>Response headers, redacted only when redaction is enabled.</summary>
    public IReadOnlyDictionary<string, string> ResponseHeaders { get; init; } = new Dictionary<string, string>();

    /// <summary>Response body, redacted only when redaction is enabled.</summary>
    public string ResponseBody { get; init; } = string.Empty;

    /// <summary>Wall-clock duration of the request.</summary>
    public TimeSpan Elapsed { get; init; }

    /// <summary>Whether the call succeeded.</summary>
    public bool IsSuccessful { get; init; }

    /// <summary>Transport or non-HTTP error message, when one occurred.</summary>
    public string ErrorMessage { get; init; } = string.Empty;
}
