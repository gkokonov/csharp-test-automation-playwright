using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

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
