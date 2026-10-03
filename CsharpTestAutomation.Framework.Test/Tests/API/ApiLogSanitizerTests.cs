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
    public void Verify_AuthorizationHeaderRedacted_When_SanitizingHeaders()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Authorization"] = "Bearer abc"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["Authorization"].Should().Be("***REDACTED***");
    }

    [Test]
    public void Verify_CookieHeaderRedacted_When_SanitizingHeaders()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Cookie"] = "session=xyz"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["Cookie"].Should().Be("***REDACTED***");
    }

    [Test]
    public void Verify_UnknownHeaderPassedThrough_When_SanitizingHeaders()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["X-Custom"] = "value"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["X-Custom"].Should().Be("value");
    }

    [Test]
    public void Verify_ConfiguredHeaderRedacted_When_SanitizingHeaders()
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
    public void Verify_HeaderRedactedCaseInsensitively_When_SanitizingHeaders()
    {
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["authorization"] = "token"
        };

        IReadOnlyDictionary<string, string> result = _sanitizer.SanitizeHeaders(headers);

        result["authorization"].Should().Be("***REDACTED***");
    }

    [Test]
    public void Verify_PasswordFieldRedacted_When_SanitizingBody()
    {
        const string body = """{"name":"Alice","password":"s3cr3t"}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("s3cr3t");
        sanitized.Should().Contain("Alice");
    }

    [Test]
    public void Verify_NestedAccessTokenRedacted_When_SanitizingBody()
    {
        const string body = """{"data":{"access_token":"abc"}}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("abc");
    }

    [Test]
    public void Verify_NonSensitiveFieldPassedThrough_When_SanitizingBody()
    {
        const string body = """{"name":"Alice"}""";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("Alice");
        sanitized.Should().NotContain("***REDACTED***");
    }

    [Test]
    public void Verify_OriginalBodyReturned_When_JsonIsInvalid()
    {
        const string body = "not json";

        var sanitized = _sanitizer.SanitizeBody(body);

        sanitized.Should().Be(body);
    }

    [Test]
    public void Verify_NullReturned_When_BodyIsNull()
    {
        var sanitized = _sanitizer.SanitizeBody(null!);

        sanitized.Should().BeNull();
    }

    [Test]
    public void Verify_ConfiguredFieldRedacted_When_SanitizingBody()
    {
        var settings = new ApiLoggingSettings { RedactSensitiveData = true, AdditionalRedactedFields = ["mySecret"] };
        var sanitizer = new ApiLogSanitizer(settings);
        const string body = """{"mySecret":"x"}""";

        var sanitized = sanitizer.SanitizeBody(body);

        sanitized.Should().Contain("***REDACTED***");
        sanitized.Should().NotContain("\"x\"");
    }

    [Test]
    public void Verify_SensitiveHeaderPassedThrough_When_RedactionIsDisabled()
    {
        var sanitizer = new ApiLogSanitizer(new ApiLoggingSettings { RedactSensitiveData = false });
        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string> {
            ["Authorization"] = "Bearer abc"
        };

        IReadOnlyDictionary<string, string> result = sanitizer.SanitizeHeaders(headers);

        result["Authorization"].Should().Be("Bearer abc");
    }

    [Test]
    public void Verify_SensitiveFieldPassedThrough_When_RedactionIsDisabled()
    {
        var sanitizer = new ApiLogSanitizer(new ApiLoggingSettings { RedactSensitiveData = false });
        const string body = """{"name":"Alice","password":"s3cr3t"}""";

        var sanitized = sanitizer.SanitizeBody(body);

        sanitized.Should().Be(body);
        sanitized.Should().NotContain("***REDACTED***");
    }

    [Test]
    public void Verify_SensitiveDetailsNotRetained_When_UsingDefaultLoggingSettings()
    {
        var settings = new ApiLoggingSettings();

        settings.RedactSensitiveData.Should().BeTrue();
        settings.AttachOnFailureOnly.Should().BeTrue();
        settings.LogFullDetail.Should().BeFalse();
    }
}
