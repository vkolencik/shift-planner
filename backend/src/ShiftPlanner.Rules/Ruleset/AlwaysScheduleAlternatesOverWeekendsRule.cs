using ShiftPlanner.Domain;

namespace ShiftPlanner.Rules.Ruleset;

public class AlwaysScheduleAlternatesOverWeekendsRule : IRule
{
    private readonly IMonthScheduleParameters _parameters;

    public AlwaysScheduleAlternatesOverWeekendsRule(IMonthScheduleParameters parameters)
    {
        _parameters = parameters;
    }

    public void Apply(IScheduleConstraintReceiver target)
    {
        var fridays = _parameters.Days.Where(d => d.DayOfWeek == DayOfWeek.Friday).ToArray();
        var sundaysAfterFridays = fridays.Select(d => new Day(d.Date.AddDays(2))).Where(d => d.Date.Month == _parameters.Month.Month).ToArray();

        foreach (var employeeMonth in _parameters.EmployeeMonths)
        {
            foreach (var (friday, sunday) in fridays.Zip(sundaysAfterFridays))
            {
                foreach (var workplace in _parameters.Workplaces)
                {
                    var interchangeabilityGroup = _parameters.WeekendInterchangeabilityGroups.SingleOrDefault(ig => ig.Contains(workplace)) ?? new HashSet<Workplace> {workplace};

                    target.AddSumRelationConstraint(
                        interchangeabilityGroup.Select(w => new ShiftAssignment(employeeMonth.Employee, friday, w)).ToArray(),
                        Constraint.Eq,
                        interchangeabilityGroup.Select(w => new ShiftAssignment(employeeMonth.Employee, sunday, w)).ToArray());
                }
            }
        }

        void EnforceWeekendAlternatesFromLastMonth(Day lastMonthDay)
        {
            if (lastMonthDay.DayOfWeek == DayOfWeek.Friday)
            {
                var shiftsOnFriday = _parameters.ActiveEmployeeLastMonthShifts.Where(s => s.Shift.Day == lastMonthDay).Where(s => _parameters.Employees.Contains(s.Employee));
                var sunday = new Day(lastMonthDay.Date.AddDays(2));
                foreach (var fridayShift in shiftsOnFriday)
                {
                    var workplace = fridayShift.Shift.Workplace;
                    var interchangeabilityGroup = _parameters.WeekendInterchangeabilityGroups.SingleOrDefault(ig => ig.Contains(workplace)) ?? new HashSet<Workplace> {workplace};
                    target.AddSumConstraint(interchangeabilityGroup.Select(w => new ShiftAssignment(fridayShift.Employee, sunday, w)).ToArray(), Constraint.Eq, 1);
                }
            }
        }

        var previousMonthDays = _parameters.Month.Previous.Days;
        EnforceWeekendAlternatesFromLastMonth(previousMonthDays.Last());
        EnforceWeekendAlternatesFromLastMonth(previousMonthDays.ElementAt(previousMonthDays.Count - 2));
    }
}
