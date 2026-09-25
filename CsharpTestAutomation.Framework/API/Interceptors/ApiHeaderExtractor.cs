using RestSharp;

namespace CsharpTestAutomation.Framework.API.Interceptors;

/// <summary>
/// Extracts request and response headers from a RestSharp <see cref="RestResponseBase"/> into a
/// case-insensitive dictionary. Values are returned <b>raw</b>; redaction (when enabled) is applied
/// separately by the logging layer, never here. Shared by the logging interceptor and the API
/// assertion helpers to avoid duplicating extraction logic.
/// </summary>
internal static class ApiHeaderExtractor
{
    /// <summary>Extracts the request headers that were sent, read from <see cref="RestResponseBase.MergedParameters"/>.</summary>
    public static IReadOnlyDictionary<string, string> ExtractRequestHeaders(RestResponseBase response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (Parameter parameter in response.MergedParameters)
        {
            if (parameter.Type == ParameterType.HttpHeader && parameter.Name is not null)
            {
                headers[parameter.Name] = parameter.Value?.ToString() ?? string.Empty;
            }
        }

        return headers;
    }

    /// <summary>Extracts the response headers, combining <see cref="RestResponseBase.Headers"/> and content headers.</summary>
    public static IReadOnlyDictionary<string, string> ExtractResponseHeaders(RestResponseBase response)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        AddHeaders(headers, response.Headers);
        AddHeaders(headers, response.ContentHeaders);

        return headers;
    }

    private static void AddHeaders(Dictionary<string, string> target, IReadOnlyCollection<HeaderParameter> source)
    {
        if (source is null)
        {
            return;
        }

        foreach (HeaderParameter header in source)
        {
            if (header.Name is not null)
            {
                target[header.Name] = header.Value?.ToString() ?? string.Empty;
            }
        }
    }
}
