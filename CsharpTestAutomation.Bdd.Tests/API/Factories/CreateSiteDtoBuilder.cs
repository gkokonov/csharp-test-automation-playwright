using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class CreateSiteDtoBuilder : BaseBuilder<CreateSiteDto>
{
    public override BaseBuilder<CreateSiteDto> Default()
    {
        var name = NetBoxTestData.NewName("site");

        With(x => x.Name = name);
        With(x => x.Slug = name);
        With(x => x.Status = NetBoxTestData.ActiveStatus);
        With(x => x.Description = NetBoxTestData.DefaultDescription);

        return this;
    }
}
