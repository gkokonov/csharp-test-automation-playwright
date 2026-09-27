using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class CreatePrefixDtoBuilder : BaseBuilder<CreatePrefixDto>
{
    public override BaseBuilder<CreatePrefixDto> Default()
    {
        With(x => x.Prefix = NetBoxTestData.NewPrefix());
        With(x => x.Status = NetBoxTestData.ActiveStatus);
        With(x => x.Description = NetBoxTestData.DefaultDescription);

        return this;
    }
}
