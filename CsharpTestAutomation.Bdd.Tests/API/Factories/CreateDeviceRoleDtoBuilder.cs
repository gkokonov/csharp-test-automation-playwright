using CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

public class CreateDeviceRoleDtoBuilder : BaseBuilder<CreateDeviceRoleDto>
{
    public override BaseBuilder<CreateDeviceRoleDto> Default()
    {
        string name = NetBoxTestData.NewName("role");

        With(x => x.Name = name);
        With(x => x.Slug = name);
        With(x => x.Color = "000000");

        return this;
    }
}
