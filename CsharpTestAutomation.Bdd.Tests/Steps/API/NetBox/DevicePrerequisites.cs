using CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceRoles;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.DeviceTypes;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Manufacturers;
using CsharpTestAutomation.Bdd.Tests.Api.Dtos.Sites;

namespace CsharpTestAutomation.Bdd.Tests.Steps.Api.NetBox;

public sealed record DevicePrerequisites(
    ManufacturerResponseDto Manufacturer,
    DeviceTypeResponseDto DeviceType,
    DeviceRoleResponseDto Role,
    SiteDetailDto Site);
