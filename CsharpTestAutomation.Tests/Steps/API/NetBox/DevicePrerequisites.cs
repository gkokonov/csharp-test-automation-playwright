using CsharpTestAutomation.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Tests.Api.Dtos.Sites;

namespace CsharpTestAutomation.Tests.Steps.Api.NetBox;

public sealed record DevicePrerequisites(
    ManufacturerDto Manufacturer,
    DeviceTypeDto DeviceType,
    DeviceRoleDto Role,
    SiteDetailDto Site);
