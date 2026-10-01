using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class UpdateSiteDtoBuilder : BaseBuilder<UpdateSiteDto>
{
    public override BaseBuilder<UpdateSiteDto> Default()
    {
        With(x => x.Status = NetBoxTestData.PlannedStatus);
        With(x => x.Description = NetBoxTestData.UpdatedDescription);

        return this;
    }
}
