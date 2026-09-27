using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

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
