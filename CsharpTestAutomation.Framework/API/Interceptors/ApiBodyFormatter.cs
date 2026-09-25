using System.Text.Json;
using CsharpTestAutomation.Framework.Common.Extensions;
using RestSharp;

namespace CsharpTestAutomation.Framework.API.Interceptors;

/// <summary>
/// Serializes request bodies to their <b>raw</b> string form for inclusion in logging.
/// Strings pass through unchanged; other objects are
/// serialized with the shared <see cref="JsonExtensions.DefaultOptions"/>. Truncation and redaction
/// are applied separately by the logging layer, never here.
/// </summary>
internal static class ApiBodyFormatter
{
    /// <summary>Serializes <paramref name="body"/> to JSON, or returns it unchanged when it is already a string.</summary>
    public static string Serialize(object body) => body switch {
        null => string.Empty,
        string s => s,
        _ => JsonSerializer.Serialize(body, JsonExtensions.DefaultOptions)
    };

    /// <summary>
    /// Reads the request body parameter from a RestSharp response and returns its raw string form.
    /// Used by the logging interceptor, which only has access to the executed response.
    /// </summary>
    public static string ExtractRequestBody(RestResponseBase response)
    {
        var value = response.Request?.Parameters
            .FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value;

        return Serialize(value);
    }
}
