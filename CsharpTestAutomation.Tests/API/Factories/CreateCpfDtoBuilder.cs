using CsharpTestAutomation.Framework.Common.Utilities;
using CsharpTestAutomation.Tests.Api.Dtos;

namespace CsharpTestAutomation.Tests.Api.Factories;

public class CreateCpfDtoBuilder : BaseBuilder<CreateCpfDto>
{
    /// <summary>
    /// Applies sensible defaults for all fields. Override individual properties with
    /// <see cref="BaseBuilder{TModel}.With{TProperty}"/> before calling
    /// <see cref="BaseBuilder{TModel}.Build"/>. Note that <c>CountryCode</c> and
    /// <c>TeamLeadId</c> have no built-in defaults — supply them via <c>With</c>.
    /// </summary>
    public override BaseBuilder<CreateCpfDto> Default()
    {
        var startYear = DateTime.UtcNow.Year;
        return With(x => x.Title = $"Automation CPF {Guid.NewGuid():N}")
            .With(x => x.Description = "Created by automated POST /api/cpfs positive test.")
            .With(x => x.CoverageStartYear = startYear)
            .With(x => x.CoverageEndYear = startYear + 5);
    }
}
