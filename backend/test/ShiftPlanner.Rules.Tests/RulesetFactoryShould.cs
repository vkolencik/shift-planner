using System.Reflection;
using Moq;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Tests;

public class RulesetFactoryShould
{
    [Fact]
    public void ReturnAllRulesInRuleset()
    {
        var ruleset = RulesetFactory.CreateRuleset(
            new Mock<IMonthScheduleParameters>().Object,
            ScheduleFlexibility.Strict, 
            new Mock<ISchedulingParametersRecorder>().Object);

        var allRuleTypes = Assembly.GetAssembly(typeof(RulesetFactory))?
            .GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IRule)))
            .ToArray();

        Assert.NotNull(allRuleTypes);
        Assert.NotEmpty(allRuleTypes);
        Assert.All(allRuleTypes, rt => Assert.Contains(ruleset, r => r.GetType() == rt));
    }
}
