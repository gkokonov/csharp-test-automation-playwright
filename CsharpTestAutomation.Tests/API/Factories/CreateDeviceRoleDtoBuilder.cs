using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class CreateDeviceRoleDtoBuilder : BaseBuilder<CreateDeviceRoleDto>
{
    public override BaseBuilder<CreateDeviceRoleDto> Default()
    {
        var name = NetBoxTestData.NewName("role");

        With(x => x.Name = name);
        With(x => x.Slug = name);
        With(x => x.Color = "000000");

        return this;
    }
}
