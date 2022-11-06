using ShiftPlanner.Domain;

namespace ShiftPlanner.Api.Tests;

internal static class TestData
{
    public static Dictionary<Day, string> Requirements = new Dictionary<Day, string>
    {
        [new Day(2022, 10, 12)] = "hard-no"
    };
}
