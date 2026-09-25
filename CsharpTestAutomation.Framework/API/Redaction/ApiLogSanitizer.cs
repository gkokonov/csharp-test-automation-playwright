using CsharpTestAutomation.Framework.API.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace CsharpTestAutomation.Framework.API.Redaction;

/// <summary>
/// Default <see cref="IApiLogSanitizer"/> implementation. When redaction is enabled via
/// <see cref="ApiLoggingSettings.RedactSensitiveData"/>, replaces a configurable set of sensitive
/// HTTP headers and JSON body fields with <c>***REDACTED***</c>; otherwise both methods pass their
/// input through unchanged. Both methods are defensive and never throw.
/// </summary>
/// <param name="settings">Logging settings controlling whether redaction is applied and which body fields to redact.</param>
public sealed class ApiLogSanitizer(ApiLoggingSettings settings) : IApiLogSanitizer
{
    private const string RedactedValue = "***REDACTED***";

    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();

    private static readonly HashSet<string> s_redactedHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Proxy-Authorization",
        "Cookie",
        "Set-Cookie"
    };

    private readonly bool _redactionEnabled = settings.RedactSensitiveData;

    private readonly HashSet<string> _redactedBodyFields = BuildBodyFields(settings);

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> SanitizeHeaders(IReadOnlyDictionary<string, string> headers)
    {
        if (!_redactionEnabled)
        {
            return headers;
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                result[header.Key] = s_redactedHeaders.Contains(header.Key) ? RedactedValue : header.Value;
            }
        }
        catch (Exception ex)
        {
            s_log.Warn(ex, "Failed to sanitize headers; returning best-effort copy.");
        }

        return result;
    }

    /// <inheritdoc />
    public string SanitizeBody(string body)
    {
        if (!_redactionEnabled || string.IsNullOrWhiteSpace(body))
        {
            return body;
        }

        try
        {
            var token = JToken.Parse(body);
            RedactToken(token);
            return token.ToString(Formatting.Indented);
        }
        catch (JsonException ex)
        {
            s_log.Warn(ex, "Body is not valid JSON; returning original content unchanged.");
            return body;
        }
        catch (Exception ex)
        {
            s_log.Warn(ex, "Failed to sanitize body; returning original content unchanged.");
            return body;
        }
    }

    private void RedactToken(JToken token)
    {
        switch (token)
        {
            case JObject obj:
                foreach (JProperty property in obj.Properties())
                {
                    if (_redactedBodyFields.Contains(property.Name))
                    {
                        property.Value = new JValue(RedactedValue);
                    }
                    else
                    {
                        RedactToken(property.Value);
                    }
                }

                break;

            case JArray array:
                foreach (JToken item in array)
                {
                    RedactToken(item);
                }

                break;
        }
    }

    private static HashSet<string> BuildBodyFields(ApiLoggingSettings settings)
    {
        var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password",
            "token",
            "access_token",
            "refresh_token",
            "id_token",
            "api_key",
            "apikey",
            "client_secret",
            "secret",
            "x-api-key"
        };

        if (settings.AdditionalRedactedFields is null)
        {
            return fields;
        }

        foreach (var field in settings.AdditionalRedactedFields)
        {
            if (!string.IsNullOrWhiteSpace(field))
            {
                fields.Add(field);
            }
        }

        return fields;
    }
}
