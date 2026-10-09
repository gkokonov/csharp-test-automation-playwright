using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Devices;
using CsharpTestAutomation.Bdd.Tests.TestData.NetBox;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Bdd.Tests.Api.Factories;

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
