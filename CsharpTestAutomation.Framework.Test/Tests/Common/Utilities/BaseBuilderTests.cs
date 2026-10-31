using AwesomeAssertions;
using AwesomeAssertions.Execution;
using CsharpTestAutomation.Framework.Common.Utilities;

namespace CsharpTestAutomation.Framework.Test.Tests.Common.Utilities;

[TestFixture]
public class BaseBuilderTests
{
    [TestCase(DayOfWeek.Friday)]
    [TestCase("friday")]
    [TestCase(5)]
    [TestCase(5L)]
    public void Verify_EnumValuesMapped_When_InputIsEnumNameOrInteger(object value)
    {
        var builder = new ScheduleBuilder();

        Schedule result = builder.BuildFromDictionary(new()
        {
            [nameof(Schedule.Day)] = value,
            [nameof(Schedule.OptionalDay)] = value
        });

        using (new AssertionScope())
        {
            result.Day.Should().Be(DayOfWeek.Friday);
            result.OptionalDay.Should().Be(DayOfWeek.Friday);
        }
    }

    [Test]
    public void Verify_NullPreserved_When_EnumPropertyIsNullable()
    {
        Schedule result = new ScheduleBuilder().BuildFromDictionary(new()
        {
            [nameof(Schedule.OptionalDay)] = null!
        });

        result.OptionalDay.Should().BeNull();
    }

    [TestCase("not-a-day")]
    [TestCase(5.5)]
    public void Verify_InvalidEnumInputRejected(object value)
    {
        Action build = () => new ScheduleBuilder().BuildFromDictionary(new()
        {
            [nameof(Schedule.OptionalDay)] = value
        });

        build.Should().Throw<ArgumentException>();
    }

    public sealed class Schedule
    {
        public DayOfWeek Day { get; set; }
        public DayOfWeek? OptionalDay { get; set; }
    }

    private sealed class ScheduleBuilder : BaseBuilder<Schedule>
    {
        public override BaseBuilder<Schedule> Default() =>
            With(x => x.Day = DayOfWeek.Monday).With(x => x.OptionalDay = DayOfWeek.Monday);
    }
}
