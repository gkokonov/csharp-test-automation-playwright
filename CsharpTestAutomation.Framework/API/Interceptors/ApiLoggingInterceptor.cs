using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml;
using Allure.Net.Commons;
using CsharpTestAutomation.Framework.API.Configuration;
using CsharpTestAutomation.Framework.API.Logging;
using CsharpTestAutomation.Framework.API.Redaction;
using CsharpTestAutomation.Framework.Common.Extensions;
using NLog;
using RestSharp;
using RestSharp.Interceptors;

namespace CsharpTestAutomation.Framework.API.Interceptors;

/// <summary>
/// RestSharp <see cref="Interceptor"/> that records a snapshot of every API call to NLog
/// and, when enabled, attaches request/response details to the Allure report. The interceptor
/// is defensive: any failure is logged and swallowed so it can never break test execution.
/// Log events carry the NUnit test full name in the <c>TestName</c> scope property
/// (<c>${scopeproperty:TestName}</c>) so parallel test output can be correlated.
/// </summary>
/// <param name="sanitizer">Sanitizer applied to headers and bodies before logging.</param>
/// <param name="settings">Logging settings controlling truncation and Allure attachments.</param>
public sealed class ApiLoggingInterceptor(IApiLogSanitizer sanitizer, ApiLoggingSettings settings) : Interceptor
{
    /// <summary>NLog scope property that holds the full name of the NUnit test that made the call.</summary>
    public const string TestNameScopeProperty = "TestName";

    private const string TruncationSuffix = " [TRUNCATED]";
    private const string AttachmentMimeType = "text/plain";
    private const string AttachmentExtension = ".txt";

    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    // RestSharp runs BeforeRequest in a nested async method, so an AsyncLocal set there may not reach AfterRequest.
    private readonly ConditionalWeakTable<RestRequest, StrongBox<long>> _startTimestamps = new();

    /// <inheritdoc />
    public override ValueTask BeforeRequest(RestRequest request, CancellationToken cancellationToken)
    {
        _startTimestamps.AddOrUpdate(request, new StrongBox<long>(Stopwatch.GetTimestamp()));
        return default;
    }

    /// <inheritdoc />
    public override ValueTask AfterRequest(RestResponse response, CancellationToken cancellationToken)
    {
        try
        {
            using IDisposable testScope =
                ScopeContext.PushProperty(TestNameScopeProperty, TestContext.CurrentContext.Test.FullName);

            TimeSpan elapsed = _startTimestamps.TryGetValue(response.Request, out StrongBox<long>? startTimestamp)
                ? Stopwatch.GetElapsedTime(startTimestamp.Value)
                : TimeSpan.Zero;
            _startTimestamps.Remove(response.Request);

            IReadOnlyDictionary<string, string> requestHeaders =
                sanitizer.SanitizeHeaders(ApiHeaderExtractor.ExtractRequestHeaders(response));
            IReadOnlyDictionary<string, string> responseHeaders =
                sanitizer.SanitizeHeaders(ApiHeaderExtractor.ExtractResponseHeaders(response));
            var requestBody = Truncate(sanitizer.SanitizeBody(ApiBodyFormatter.ExtractRequestBody(response)));
            var responseBody = Truncate(sanitizer.SanitizeBody(response.Content ?? string.Empty));

            var entry = new ApiLogEntry {
                Method = response.Request.Method.ToString().ToUpperInvariant(),
                Resource = response.Request.Resource,
                FullUri = response.ResponseUri,
                RequestHeaders = requestHeaders,
                RequestBody = requestBody,
                StatusCode = (int)response.StatusCode,
                ReasonPhrase = response.StatusDescription ?? string.Empty,
                ResponseHeaders = responseHeaders,
                ResponseBody = responseBody,
                Elapsed = elapsed,
                IsSuccessful = response.IsSuccessful,
                ErrorMessage = response.ErrorMessage ?? string.Empty
            };

            LogToNLog(entry, response.ErrorException);
            LogFullDetail(entry);
            AttachToAllure(entry, response.IsSuccessful);

            return default;
        }
        catch (Exception ex)
        {
            s_log.Warn(ex, "API logging interceptor failed; suppressing to protect test execution.");
            return default;
        }
    }

    private static void LogToNLog(ApiLogEntry entry, Exception? errorException)
    {
        var message =
            $"[API] {entry.Method} {entry.Resource} → {entry.StatusCode} | {entry.Elapsed.TotalMilliseconds:F0}ms";

        if (errorException is not null || entry.StatusCode >= 500)
        {
            s_log.Error(errorException, message);
        }
        else if (entry.StatusCode >= 400)
        {
            s_log.Warn(message);
        }
        else
        {
            s_log.Info(message);
        }
    }

    private void LogFullDetail(ApiLogEntry entry)
    {
        if (!settings.LogFullDetail || !s_log.IsDebugEnabled)
        {
            return;
        }

        s_log.Debug("{0}{1}{2}", BuildRequestAttachment(entry), Environment.NewLine, BuildResponseAttachment(entry));
    }

    private void AttachToAllure(ApiLogEntry entry, bool isSuccessful)
    {
        if (!settings.AttachToAllure || (settings.AttachOnFailureOnly && isSuccessful))
        {
            return;
        }

        AddAttachment($"API Request - {entry.Method} {entry.Resource}", BuildRequestAttachment(entry));
        AddAttachment($"API Response - {entry.StatusCode} {entry.Method} {entry.Resource}", BuildResponseAttachment(entry));
    }

    private static void AddAttachment(string name, string content)
    {
        try
        {
            AllureApi.AddAttachment(name, AttachmentMimeType, Encoding.UTF8.GetBytes(content), AttachmentExtension);
        }
        catch (InvalidOperationException ex)
        {
            s_log.Warn(ex, "No active Allure context; skipping attachment '{0}'.", name);
        }
        catch (Exception ex)
        {
            s_log.Warn(ex, "Failed to add Allure attachment '{0}'.", name);
        }
    }

    private static string BuildRequestAttachment(ApiLogEntry entry)
    {
        var builder = new StringBuilder();
        builder.AppendLine("=== API REQUEST ===");
        builder.AppendLine($"Time:           {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        builder.AppendLine($"Method:         {entry.Method}");
        builder.AppendLine($"URL:            {entry.FullUri?.ToString() ?? entry.Resource}");
        builder.AppendLine();
        builder.AppendLine("Headers:");
        AppendHeaders(builder, entry.RequestHeaders);

        if (!string.IsNullOrWhiteSpace(entry.RequestBody))
        {
            builder.AppendLine();
            builder.AppendLine("Body:");
            builder.AppendLine(TryFormatBody(entry.RequestBody));
        }

        return builder.ToString();
    }

    private static string BuildResponseAttachment(ApiLogEntry entry)
    {
        var builder = new StringBuilder();
        builder.AppendLine("=== API RESPONSE ===");
        builder.AppendLine($"Status:         {entry.StatusCode} {entry.ReasonPhrase}");
        builder.AppendLine($"Elapsed:        {entry.Elapsed.TotalMilliseconds:F0}ms");
        builder.AppendLine($"URL:            {entry.FullUri?.ToString() ?? entry.Resource}");
        builder.AppendLine();
        builder.AppendLine("Headers:");
        AppendHeaders(builder, entry.ResponseHeaders);

        if (!string.IsNullOrWhiteSpace(entry.ResponseBody))
        {
            builder.AppendLine();
            builder.AppendLine("Body:");
            builder.AppendLine(TryFormatBody(entry.ResponseBody));
        }

        return builder.ToString();
    }

    private static void AppendHeaders(StringBuilder builder, IReadOnlyDictionary<string, string> headers)
    {
        foreach (KeyValuePair<string, string> header in headers)
        {
            builder.AppendLine($"  {header.Key}: {header.Value}");
        }
    }

    /// <summary>
    /// Best-effort pretty-print of a body string. Detects JSON, XML, and URL-encoded form data;
    /// falls back to the original string on any parse failure so truncated or non-standard
    /// content is never lost.
    /// </summary>
    private static string TryFormatBody(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return body;
        }

        // JSON
        if ((body.StartsWith('{') && body.EndsWith('}')) ||
            (body.StartsWith('[') && body.EndsWith(']')))
        {
            try
            {
                JsonElement element = JsonSerializer.Deserialize<JsonElement>(body);
                return JsonSerializer.Serialize(element, JsonExtensions.DefaultOptions);
            }
            catch
            {
                return body;
            }
        }

        // XML
        if (body.StartsWith('<') && body.EndsWith('>'))
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(body);

                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
                doc.WriteTo(xmlWriter);
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
            catch
            {
                return body;
            }
        }

        // URL-encoded form data
        if (body.Contains('=') && body.Contains('&'))
        {
            try
            {
                var formatted = new StringBuilder();
                foreach (var pair in body.Split('&'))
                {
                    var parts = pair.Split('=', 2);
                    var key = Uri.UnescapeDataString(parts[0]);
                    var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
                    formatted.AppendLine($"{key}: {value}");
                }

                return formatted.ToString();
            }
            catch
            {
                return body;
            }
        }

        return body;
    }

    private string Truncate(string value)
    {
        var maxBytes = settings.MaxBodySizeBytes;
        if (string.IsNullOrEmpty(value) || Encoding.UTF8.GetByteCount(value) <= maxBytes)
        {
            return value;
        }

        var byteCount = 0;
        var charCount = 0;
        foreach (Rune rune in value.EnumerateRunes())
        {
            byteCount += rune.Utf8SequenceLength;
            if (byteCount > maxBytes)
            {
                break;
            }

            charCount += rune.Utf16SequenceLength;
        }

        return string.Concat(value.AsSpan(0, charCount), TruncationSuffix);
    }
}
