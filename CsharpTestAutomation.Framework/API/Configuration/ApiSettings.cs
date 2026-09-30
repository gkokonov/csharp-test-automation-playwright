namespace CsharpTestAutomation.Framework.API.Configuration;

/// <summary>
/// Root configuration block for the API testing layer. Bound from the <c>Api</c> key in
/// <c>appsettings.json</c> via <c>AppConfiguration&lt;CoreConfiguration&gt;</c>.
/// </summary>
public class ApiSettings
{
    /// <summary>Named API services keyed by a logical service name (matched by typed clients).</summary>
    public IDictionary<string, ApiServiceSettings> Services { get; set; } = new Dictionary<string, ApiServiceSettings>();

    /// <summary>Logging and Allure attachment behaviour for API calls.</summary>
    public ApiLoggingSettings Logging { get; set; } = new();
}

/// <summary>Per-service connection settings (base URL and timeout).</summary>
public class ApiServiceSettings
{
    /// <summary>Absolute base URL of the service, e.g. <c>https://api.example.com</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Request timeout in seconds applied to the underlying HTTP client.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates the service settings, throwing <see cref="InvalidOperationException"/> with the
    /// offending <paramref name="serviceName"/> and setting key when a value is missing or invalid.
    /// </summary>
    /// <param name="serviceName">Logical service name used to contextualize error messages.</param>
    /// <exception cref="InvalidOperationException">A required value is empty or out of range.</exception>
    public void Validate(string serviceName)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl))
        {
            throw new InvalidOperationException(
                $"API service '{serviceName}' has an empty Api.Services.{serviceName}.BaseUrl. " +
                "Provide an absolute base URL such as 'https://api.example.com'.");
        }

        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException(
                $"API service '{serviceName}' has an invalid Api.Services.{serviceName}.BaseUrl " +
                $"value '{BaseUrl}'. It must be an absolute URI such as 'https://api.example.com'.");
        }

        if (TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                $"API service '{serviceName}' has a non-positive Api.Services.{serviceName}.TimeoutSeconds " +
                $"value '{TimeoutSeconds}'. It must be greater than zero.");
        }
    }
}

/// <summary>Controls API request/response logging and Allure attachments.</summary>
public class ApiLoggingSettings
{
    /// <summary>When <see langword="true"/>, API request/response details are attached to the Allure report.</summary>
    public bool AttachToAllure { get; set; } = true;

    /// <summary>
    /// When <see langword="true"/>, Allure attachments are produced only for failed calls.
    /// When <see langword="false"/>, attachments are produced for every call. Defaults to
    /// <see langword="true"/> to reduce retained request and response data.
    /// </summary>
    public bool AttachOnFailureOnly { get; set; } = true;

    /// <summary>Maximum number of bytes of request/response body retained before truncation.</summary>
    public int MaxBodySizeBytes { get; set; } = 15_000;

    /// <summary>
    /// When <see langword="true"/>, full request/response headers and bodies are written to the log
    /// at Debug level after configured sanitization. Console only shows the Info summary line; the
    /// file target captures the full detail. Defaults to <see langword="false"/>.
    /// </summary>
    public bool LogFullDetail { get; set; }

    /// <summary>
    /// When <see langword="true"/>, sensitive headers and JSON body fields are replaced with
    /// <c>***REDACTED***</c> before logging or attaching to reports. Defaults to
    /// <see langword="true"/>. Set to <see langword="false"/> only when raw values are required
    /// for debugging and the logs and reports are access-controlled.
    /// </summary>
    public bool RedactSensitiveData { get; set; } = true;

    /// <summary>
    /// Additional JSON field names (appended to the built-in list) redacted from logged bodies.
    /// Only applied when <see cref="RedactSensitiveData"/> is <see langword="true"/>.
    /// </summary>
    public IList<string> AdditionalRedactedFields { get; set; } = [];

    /// <summary>
    /// Additional HTTP header names appended to the built-in redaction list, for example
    /// <c>X-Api-Key</c>. Only applied when <see cref="RedactSensitiveData"/> is <see langword="true"/>.
    /// </summary>
    public IList<string> AdditionalRedactedHeaders { get; set; } = [];

    /// <summary>
    /// Validates the logging settings, throwing <see cref="InvalidOperationException"/> when a value
    /// is out of range.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="MaxBodySizeBytes"/> is not positive.</exception>
    public void Validate()
    {
        if (MaxBodySizeBytes <= 0)
        {
            throw new InvalidOperationException(
                $"Api.Logging.MaxBodySizeBytes value '{MaxBodySizeBytes}' is invalid. " +
                "It must be greater than zero.");
        }
    }
}
