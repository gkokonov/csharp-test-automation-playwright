using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class CreateIpAddressDtoBuilder : BaseBuilder<CreateIpAddressDto>
{
    public override BaseBuilder<CreateIpAddressDto> Default()
    {
        With(x => x.Address = NetBoxTestData.NewIpAddress());
        With(x => x.Status = NetBoxTestData.ActiveStatus);
        With(x => x.Description = NetBoxTestData.DefaultDescription);

        return this;
    }
}
