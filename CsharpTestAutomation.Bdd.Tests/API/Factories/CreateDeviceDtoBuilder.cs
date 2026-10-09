using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

/// <summary>
/// <see cref="CreateDeviceDto.DeviceType"/>, <see cref="CreateDeviceDto.Role"/>, and
/// <see cref="CreateDeviceDto.Site"/> have no valid default; callers must always override them
/// with ids created via the corresponding prerequisite builders/clients.
/// </summary>
public class CreateDeviceDtoBuilder : BaseBuilder<CreateDeviceDto>
{
    public override BaseBuilder<CreateDeviceDto> Default()
    {
        With(x => x.Name = NetBoxTestData.NewName("device"));
        With(x => x.Status = NetBoxTestData.ActiveStatus);
        With(x => x.Description = NetBoxTestData.DefaultDescription);

        return this;
    }
}
