using System.Net;
using AwesomeAssertions;
using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Framework.Common.Utilities.CustomExceptions;

namespace CsharpTestAutomation.Framework.Test.Tests.Common.Utilities;

[TestFixture]
public class DependencyAvailabilityCheckerTests
{
    [Test]
    public async Task Verify_AuthGatedEndpointIsReachable_When_ServerReturnsForbidden()
    {
        using var client = new HttpClient(new StubHandler(HttpStatusCode.Forbidden));

        await DependencyAvailabilityChecker.EnsureAllAvailableAsync(client, [("API", "https://local.test/api/")]);
    }

    [Test]
    public async Task Verify_AllUnavailableDependenciesAreReported_When_ServersReturnErrors()
    {
        using var client = new HttpClient(new StubHandler(HttpStatusCode.ServiceUnavailable));
        Func<Task> check = () => DependencyAvailabilityChecker.EnsureAllAvailableAsync(client,
            [("API", "https://local.test/api/"), ("UI", "https://local.test/")]);

        DependencyUnavailableException error = (await check.Should().ThrowAsync<DependencyUnavailableException>()).Which;

        error.Message.Should().Contain("API").And.Contain("UI");
        error.InnerException.Should().BeOfType<AggregateException>().Which.InnerExceptions.Should().HaveCount(2);
    }

    private sealed class StubHandler(HttpStatusCode status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(status));
    }
}
