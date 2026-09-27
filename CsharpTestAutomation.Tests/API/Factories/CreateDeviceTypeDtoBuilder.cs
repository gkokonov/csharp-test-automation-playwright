using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

/// <summary>
/// <see cref="CreateDeviceTypeDto.Manufacturer"/> has no valid default; callers must always
/// override it with a manufacturer id created via <see cref="CreateManufacturerDtoBuilder"/>.
/// </summary>
public class CreateDeviceTypeDtoBuilder : BaseBuilder<CreateDeviceTypeDto>
{
    public override BaseBuilder<CreateDeviceTypeDto> Default()
    {
        var name = NetBoxTestData.NewName("devicetype");

        With(x => x.Model = name);
        With(x => x.Slug = name);

        return this;
    }
}
