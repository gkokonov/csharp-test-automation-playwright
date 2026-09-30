using AwesomeAssertions;
using CsharpTestAutomation.Framework.API.Configuration;
using CsharpTestAutomation.Framework.API.Redaction;

namespace CsharpTestAutomation.Framework.Test.Tests.Api;

[TestFixture]
[Category("ApiLogSanitizerTests")]
public class ApiLogSanitizerTests
{
    private ApiLogSanitizer _sanitizer = null!;

    [SetUp]
    public void SetUp() => _sanitizer = new ApiLogSanitizer(new ApiLoggingSettings { RedactSensitiveData = true });

    [Test]
    public void SanitizeHeaders_AuthorizationHeader_IsRedacted()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Authorization"] = "Bearer abc"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["Authorization"].Should().Be("***REDACTED***");
    }

    [Test]
    public void SanitizeHeaders_CookieHeader_IsRedacted()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Cookie"] = "session=xyz"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["Cookie"].Should().Be("***REDACTED***");
    }

    [Test]
    public void SanitizeHeaders_UnknownHeader_IsPassedThrough()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["X-Custom"] = "value"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["X-Custom"].Should().Be("value");
    }

    [Test]
    public void SanitizeHeaders_AdditionalConfiguredHeader_IsRedacted()
    {
        var sanitizer = new ApiLogSanitizer(new ApiLoggingSettings {
            RedactSensitiveData = true,
            AdditionalRedactedHeaders = ["X-Api-Key"]
        });
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["X-Api-Key"] = "secret"
        };

        IReadOnlyDictionary<string, string> result = sanitizer.SanitizeHeaders(headers);

        result["X-Api-Key"].Should().Be("***REDACTED***");
    }

    [Test]
    public void SanitizeHeaders_CaseInsensitiveMatch_IsRedacted()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["authorization"] = "token"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["authorization"].Should().Be("***REDACTED***");
    }

    [Test]
    public void SanitizeBody_PasswordField_IsRedacted()
    {
        const string body = """{"name":"Alice","password":"s3cr3t"}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("s3cr3t");
        sanitized.Should().Contain("Alice");
    }

    [Test]
    public void SanitizeBody_NestedAccessToken_IsRedacted()
    {
        const string body = """{"data":{"access_token":"abc"}}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("abc");
    }

    [Test]
    public void SanitizeBody_NonSensitiveField_IsPassedThrough()
    {
        const string body = """{"name":"Alice"}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("Alice");
        sanitized.Should().NotContain("***REDACTED***");
    }

    [Test]
    public void SanitizeBody_InvalidJson_ReturnsOriginal()
    {
        const string body = "not json";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Be(body);
    }

    [Test]
    public void SanitizeBody_NullBody_ReturnsNull()
    {
        var sanitized = _sanitizer.SanitizeBody(null!);

        sanitized.Should().BeNull();
    }

    [Test]
    public void SanitizeBody_AdditionalConfiguredField_IsRedacted()
    {
        var settings = new ApiLoggingSettings { RedactSensitiveData = true, AdditionalRedactedFields = ["mySecret"] };
        var sanitizer = new ApiLogSanitizer(settings);
        const string body = """{"mySecret":"x"}""";

        var sanitized = sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("\"x\"");
    }

    [Test]
    public void SanitizeHeaders_RedactionExplicitlyDisabled_PassesSensitiveHeaderThrough()
    {
        var sanitizer = new ApiLogSanitizer(new ApiLoggingSettings { RedactSensitiveData = false });
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Authorization"] = "Bearer abc"
        };

        IReadOnlyDictionary<string, string> result = sanitizer.SanitizeHeaders(headers);

        result["Authorization"].Should().Be("Bearer abc");
    }

    [Test]
    public void SanitizeBody_RedactionExplicitlyDisabled_PassesSensitiveFieldThrough()
    {
        var sanitizer = new ApiLogSanitizer(new ApiLoggingSettings { RedactSensitiveData = false });
        const string body = """{"name":"Alice","password":"s3cr3t"}""";

        var sanitized = sanitizer.SanitizeBody(body);

        sanitized.Should().Be(body);
        sanitized.Should().NotContain("***REDACTED***");
    }

    [Test]
    public void ApiLoggingSettings_Defaults_AvoidRetainingSensitiveDetails()
    {
        var settings = new ApiLoggingSettings();

        settings.RedactSensitiveData.Should().BeTrue();
        settings.AttachOnFailureOnly.Should().BeTrue();
        settings.LogFullDetail.Should().BeFalse();
    }
}
