using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;
using ShiftPlanner.Rules;

namespace ShiftPlanner.Engine;

internal static class Rules
{
    public static void EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(MonthScheduleParameters monthScheduleParameters, IReadOnlyCollection<Day> days, ISchedulingEngine engine)
    {
        foreach (var competenceGroup in monthScheduleParameters.CompetenceGroups)
        {
            foreach (var day in days)
            {
                IReadOnlyCollection<ShiftAssignment> employeesLevel1AndWorkplacesCombinations = monthScheduleParameters.Employees
                    .Where(e => e.CompetenceLevel == CompetenceLevel.Level1)
                    .SelectMany(e => competenceGroup.Workplaces.Select(w => new ShiftAssignment(e, day, w)))
                    .ToArray();

                engine.AddSumConstraint(employeesLevel1AndWorkplacesCombinations, Constraint.Gte, 1);
            }
        }
    }

    public static void DisallowShiftsWithoutEmployeeCapability(ISolverParameters solverParameters, ISchedulingEngine engine)
    {
        foreach (var employee in solverParameters.Employees)
        {
            var incapableWorkplaces = solverParameters.Workplaces.Except(employee.Capabilities.AllowedWorkplaces);

            foreach (var incapableWorkplace in incapableWorkplaces)
            {
                foreach (var day in solverParameters.Days)
                {
                    engine.AddVariableConstraint(new ShiftAssignment(employee, day, incapableWorkplace), Constraint.Eq, 0);
                }
            }
        }
    }

    public static void TakeLeavesIntoAccount(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        foreach (var employeeMonth in monthScheduleParameters.EmployeeMonths)
        {
            foreach (var leave in employeeMonth.Leaves)
            {
                foreach (var day in leave.Days)
                {
                    foreach (var workplace in monthScheduleParameters.Workplaces)
                    {
                        engine.AddVariableConstraint(new ShiftAssignment(employeeMonth.Employee, day, workplace), Constraint.Eq, 0);
                    }
                }
            }
        }
    }

    public static void EnforceFixedShifts(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        foreach (var fixedShift in monthScheduleParameters.FixedShifts)
        {
            engine.AddVariableConstraint(fixedShift, Constraint.Eq, 1);
        }
    }

    public static void AvoidSubsequentWeekends(MonthScheduleParameters monthScheduleParameters, ISchedulingEngine engine)
    {
        var weekendDays = monthScheduleParameters.Days.Where(d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday).ToArray();

        var saturdays = weekendDays
            .Concat(weekendDays.Where(d => d.DayOfWeek is DayOfWeek.Saturday).Select(d => new Day(d.Date.AddDays(1))))
            .Concat(weekendDays.Where(d => d.DayOfWeek is DayOfWeek.Sunday).Select(d => new Day(d.Date.AddDays(-1))))
            .Where(d => d.DayOfWeek is DayOfWeek.Saturday)
            .ToHashSet()
            .OrderBy(_ => _)
            .ToList(); // might also include last saturday of last month
        
        foreach (var employee in monthScheduleParameters.Employees)
        {
            foreach (var saturday in saturdays)
            {
                var sunday = new Day(saturday.Date.AddDays(1));
                var nextSaturday = new Day(saturday.Date.AddDays(7));
                var nextSunday = new Day(sunday.Date.AddDays(7));
                if (nextSaturday.Month != monthScheduleParameters.Month.Month) continue;
                
                var shiftsOverSubsequentWeekends = new[] {saturday, sunday, nextSaturday, nextSunday}
                    .Where(d => d.Month == monthScheduleParameters.Month.Month)
                    .SelectMany(d => monthScheduleParameters.Workplaces.Select(w => new ShiftAssignment(employee, d, w)))
                    .ToArray();
                    
                engine.AddSumConstraint(shiftsOverSubsequentWeekends, Constraint.Lte, 1);
            }
        }

        void LimitShiftsOnWeekend(Day saturdayOfLastMonth)
        {
            var lastWeekendOfLastMonth = new[]
            {
                saturdayOfLastMonth,
                new Day(saturdayOfLastMonth.Date.AddDays(1))
            }.Where(d => d.Month == monthScheduleParameters.Month.Previous.Month);

            var subsequentWeekendSaturday = new Day(saturdayOfLastMonth.Date.AddDays(7));
            var subsequentWeekend = new[] {subsequentWeekendSaturday, new Day(subsequentWeekendSaturday.Date.AddDays(1))}
                .Where(d => d.Month == monthScheduleParameters.Month.Month)
                .ToArray();

            if (!subsequentWeekend.Any())
            {
                return;
            }
            
            var lastMonthWeekendShifts = monthScheduleParameters.LastMonthShifts.Where(s => lastWeekendOfLastMonth.Contains(s.Shift.Day));
            foreach (var employee in lastMonthWeekendShifts.Select(s => s.Employee).ToHashSet())
            {
                engine.AddSumConstraint(monthScheduleParameters.Workplaces.SelectMany(w => subsequentWeekend.Select(d => new ShiftAssignment(employee, d, w))).ToArray(), Constraint.Eq, 0);
            }
        }
        
        var lastSaturdays = monthScheduleParameters.Month.Previous.Days.Reverse().Where(d => d.DayOfWeek is DayOfWeek.Saturday).ToArray();

        var lastSaturdayOfLastMonth = lastSaturdays.ElementAt(0);
        LimitShiftsOnWeekend(lastSaturdayOfLastMonth);

        var penultimateSaturdayOfLastMonth = lastSaturdays.ElementAt(1);
        LimitShiftsOnWeekend(penultimateSaturdayOfLastMonth);
    }
}
