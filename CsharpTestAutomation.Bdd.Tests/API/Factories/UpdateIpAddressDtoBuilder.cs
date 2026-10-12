using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class UpdateIpAddressDtoBuilder : BaseBuilder<UpdateIpAddressDto>
{
    public override BaseBuilder<UpdateIpAddressDto> Default()
    {
        With(x => x.Status = NetBoxTestData.ReservedStatus);
        With(x => x.Description = NetBoxTestData.UpdatedDescription);
        return this;
    }
}
