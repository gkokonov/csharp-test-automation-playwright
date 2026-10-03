using CsharpTestAutomation.Framework.Common;
using Microsoft.Extensions.Configuration;

namespace CsharpTestAutomation.Framework.Test.Tests.Common;

[TestFixture]
[Category("AppConfigurationTests")]
public class AppConfigurationTests
{
    private const string Key = "LayerProbe";
    private string _basePath = null!;

    [SetUp]
    public void SetUp()
    {
        _basePath = Path.Combine(Path.GetTempPath(), $"csharp-framework-config-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_basePath);
    }

    [TearDown]
    public void TearDown() => Directory.Delete(_basePath, recursive: true);

    [Test]
    public void Verify_LocalFileOverridesBase_When_NoEnvironmentIsSet()
    {
        WriteSettings("appsettings.json", "base");
        WriteSettings("appsettings.local.json", "local");

        IConfiguration configuration = Build(environmentName: null);

        Assert.That(configuration[Key], Is.EqualTo("local"));
    }

    [Test]
    public void Verify_EnvironmentFileOverridesBase_When_EnvironmentIsSet()
    {
        WriteSettings("appsettings.json", "base", ("BaseOnly", "kept"));
        WriteSettings("appsettings.QA.json", "qa");

        IConfiguration configuration = Build("QA");

        Assert.That(configuration[Key], Is.EqualTo("qa"));
        Assert.That(configuration["BaseOnly"], Is.EqualTo("kept"));
    }

    [Test]
    public void Verify_EnvironmentFileLoaded_When_BaseFileIsMissing()
    {
        WriteSettings("appsettings.QA.json", "qa");

        IConfiguration configuration = Build("QA");

        Assert.That(configuration[Key], Is.EqualTo("qa"));
    }

    [Test]
    public void Verify_ExceptionThrown_When_BaseFileIsMissingAndNoEnvironmentIsSet()
    {
        Assert.That(() => Build(environmentName: null), Throws.TypeOf<FileNotFoundException>());
    }

    private IConfiguration Build(string? environmentName) =>
        AppConfiguration<CoreConfiguration>.BuildDefaultConfiguration(_basePath, environmentName);

    private void WriteSettings(string fileName, string probeValue, params (string Key, string Value)[] extra)
    {
        IEnumerable<string> entries = extra
            .Select(entry => $"\"{entry.Key}\": \"{entry.Value}\"")
            .Prepend($"\"{Key}\": \"{probeValue}\"");

        File.WriteAllText(Path.Combine(_basePath, fileName), $"{{ {string.Join(", ", entries)} }}");
    }
}
