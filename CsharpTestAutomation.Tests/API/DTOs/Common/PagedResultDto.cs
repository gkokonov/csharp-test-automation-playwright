using System.Text.Json.Serialization;

namespace CsharpTestAutomation.Tests.Api.Dtos.Common;

/// <summary>
/// NetBox's paginated list envelope, shared by every collection endpoint.
/// </summary>
public sealed record PagedResultDto<T>
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("results")]
    public List<T> Results { get; set; } = [];
}
