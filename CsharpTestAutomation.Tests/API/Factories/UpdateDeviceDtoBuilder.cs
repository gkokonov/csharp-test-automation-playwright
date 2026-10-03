using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Tests.TestData.NetBox;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class UpdateDeviceDtoBuilder : BaseBuilder<UpdateDeviceDto>
{
    public override BaseBuilder<UpdateDeviceDto> Default()
    {
        With(x => x.Site = null);
        With(x => x.Status = NetBoxTestData.PlannedStatus);
        With(x => x.Description = NetBoxTestData.UpdatedDescription);
        return this;
    }
}
