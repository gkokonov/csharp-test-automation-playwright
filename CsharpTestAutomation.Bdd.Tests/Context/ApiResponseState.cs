using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Common;
using RestSharp;

namespace CsharpTestAutomation.Bdd.Tests.Context;

/// <summary>Mutable API responses owned by one feature state within one scenario.</summary>
public sealed class ApiResponseState<TDetail> where TDetail : class
{
    public RestResponse<TDetail> DetailResponse { get; set; } = null!;

    public RestResponse<PagedResultDto<TDetail>> SearchResponse { get; set; } = null!;

    public RestResponse DeleteResponse { get; set; } = null!;
}
