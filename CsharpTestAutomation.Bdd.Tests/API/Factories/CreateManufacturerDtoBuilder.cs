using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class CreateManufacturerDtoBuilder : BaseBuilder<CreateManufacturerDto>
{
    public override BaseBuilder<CreateManufacturerDto> Default()
    {
        string name = NetBoxTestData.NewName("manufacturer");

        With(x => x.Name = name);
        With(x => x.Slug = name);

        return this;
    }
}
