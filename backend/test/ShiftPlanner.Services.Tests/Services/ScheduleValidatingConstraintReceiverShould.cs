using ShiftPlanner.Domain;
using ShiftPlanner.Rules;
using ShiftPlanner.Services.Contracts;
using ShiftPlanner.Services.Services;

namespace ShiftPlanner.Services.Tests.Services;

public abstract class ScheduleValidatingConstraintReceiverShould
{
    private static readonly Workplace Workplace1 = new Workplace("workplace1");
    private static readonly Workplace Workplace2 = new Workplace("workplace2");
    private static readonly Day Day1 = new Day(2023, 01, 01);
    private static readonly Day Day2 = new Day(2023, 01, 02);
    private static readonly User User1 = new User("user1", "User 1", "user1@krtek.me");
    private static readonly Employee Employee1 = new Employee("User 1", CompetenceLevel.Level1, Capabilities, Workload.Infinite);
    private static readonly User User2 = new User("user2", "User 2", "user2@krtek.me");
    private static readonly Employee Employee2 = new Employee("User 2", CompetenceLevel.Level1, Capabilities, Workload.Infinite);
    private static readonly User User3 = new User("user3", "User 3", "user3@krtek.me");
    private static readonly Employee Employee3 = new Employee("User 3", CompetenceLevel.Level1, Capabilities, Workload.Infinite);

    private static readonly Schedule Schedule = new Schedule(
        new[]
        {
            new ScheduledShift(Day1, Workplace1, User1),
            new ScheduledShift(Day1, Workplace2, User2),
            new ScheduledShift(Day2, Workplace1, User1),
            new ScheduledShift(Day2, Workplace2, User3),
        });

    private static readonly Capabilities Capabilities = new Capabilities(new[] {new WorkplaceCapability.Green(Workplace1, 100)});

    public class SumConstraint : ScheduleValidatingConstraintReceiverShould
    {
        [Theory]
        [InlineData(Constraint.Eq, 2)]
        [InlineData(Constraint.Gte, 1)]
        [InlineData(Constraint.Gte, 2)]
        [InlineData(Constraint.Lte, 2)]
        [InlineData(Constraint.Lte, 3)]
        public void NotAddErrors_GivenValidSumConstraint(Constraint constraint, uint bound)
        {
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, ScheduleErrorType.Unknown);
            receiver.AddSumConstraint(
                new[]
                {
                    new ShiftAssignment(Employee1, Day1, Workplace1),
                    new ShiftAssignment(Employee2, Day1, Workplace2),
                    new ShiftAssignment(Employee2, Day2, Workplace1),
                }, constraint, bound);

            Assert.Empty(receiver.GetErrors());
        }

        [Theory]
        [InlineData(Constraint.Eq, 3)]
        [InlineData(Constraint.Gte, 3)]
        [InlineData(Constraint.Lte, 1)]
        public void AddError_GivenInvalidSumConstraint(Constraint constraint, uint bound)
        {
            var errorType = ScheduleErrorType.EveryDayIsCovered;
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, errorType);

            receiver.AddSumConstraint(
                new[]
                {
                    new ShiftAssignment(Employee1, Day1, Workplace1),
                    new ShiftAssignment(Employee2, Day1, Workplace2),
                    new ShiftAssignment(Employee2, Day2, Workplace1),
                }, constraint, bound);

            var error = Assert.Single(receiver.GetErrors());

            Assert.Equal(
                new[]
                {
                    new Shift(Day1, Workplace1),
                    new Shift(Day1, Workplace2),
                }, error.Shifts);
            Assert.Null(error.User);
            Assert.Equal(errorType, error.ErrorType);
        }
    }

    public class VariableConstraint : ScheduleValidatingConstraintReceiverShould
    {
        [Theory]
        [InlineData(Constraint.Eq, 1)]
        [InlineData(Constraint.Gte, 0)]
        [InlineData(Constraint.Gte, 1)]
        [InlineData(Constraint.Lte, 1)]
        [InlineData(Constraint.Lte, 2)]
        public void NotAddErrors_GivenValidVariableConstraint(Constraint constraint, uint bound)
        {
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, ScheduleErrorType.Unknown);
            receiver.AddVariableConstraint(new ShiftAssignment(Employee1, Day1, Workplace1), constraint, bound);

            Assert.Empty(receiver.GetErrors());
        }

        [Theory]
        [InlineData(Constraint.Eq, 1)]
        [InlineData(Constraint.Gte, 1)]
        public void AddError_GivenInvalidVariableConstraint(Constraint constraint, uint bound)
        {
            var errorType = ScheduleErrorType.NoConcurrentShifts;
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, errorType);

            receiver.AddVariableConstraint(new ShiftAssignment(Employee1, Day1, Workplace2), constraint, bound);

            var error = Assert.Single(receiver.GetErrors());

            Assert.Equal(
                new[]
                {
                    new Shift(Day1, Workplace2),
                }, error.Shifts);
            Assert.Null(error.User);
            Assert.Equal(errorType, error.ErrorType);
        }
    }
    
    public class SumRelationshipConstraint : ScheduleValidatingConstraintReceiverShould
    {
        [Fact]
        public void NotAddErrors_GivenValidSumConstraint()
        {
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, ScheduleErrorType.Unknown);
            
            receiver.AddSumRelationConstraint(
                new[] {new ShiftAssignment(Employee1, Day1, Workplace1)},
                Constraint.Eq,
                new[] {new ShiftAssignment(Employee1, Day2, Workplace1)});

            Assert.Empty(receiver.GetErrors());
        }

        [Fact]
        public void AddError_GivenInvalidSumConstraint()
        {
            var errorType = ScheduleErrorType.EveryDayIsCovered;
            var receiver = new ScheduleValidatingConstraintReceiver(Schedule, errorType);

            receiver.AddSumRelationConstraint(
                new[] {new ShiftAssignment(Employee1, Day1, Workplace1)},
                Constraint.Eq,
                new[] {new ShiftAssignment(Employee1, Day2, Workplace2)});

            var error = Assert.Single(receiver.GetErrors());

            Assert.Equal(
                new[]
                {
                    new Shift(Day1, Workplace1),
                    new Shift(Day2, Workplace2),
                }, error.Shifts);
            Assert.Null(error.User);
            Assert.Equal(errorType, error.ErrorType);
        }
    }
}
