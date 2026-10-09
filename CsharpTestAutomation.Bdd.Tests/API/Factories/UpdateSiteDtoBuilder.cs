using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class UpdateSiteDtoBuilder : BaseBuilder<UpdateSiteDto>
{
    public override BaseBuilder<UpdateSiteDto> Default()
    {
        With(x => x.Status = NetBoxTestData.PlannedStatus);
        With(x => x.Description = NetBoxTestData.UpdatedDescription);

        return this;
    }
}
