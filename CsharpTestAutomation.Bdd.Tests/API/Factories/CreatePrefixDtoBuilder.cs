using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

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
