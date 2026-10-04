using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Ipam;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class UpdatePrefixDtoBuilder : BaseBuilder<UpdatePrefixDto>
{
    public override BaseBuilder<UpdatePrefixDto> Default()
    {
        With(x => x.Status = NetBoxTestData.ReservedStatus);
        With(x => x.Description = NetBoxTestData.UpdatedDescription);
        return this;
    }
}
