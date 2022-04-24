using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine;

namespace ShiftPlanner.Engine;

internal static class Rules
{
    public static void EveryDayAtEveryWorkplaceIsCovered(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var day in schedulingParameters.Days)
        {
            foreach (var workplace in schedulingParameters.Workplaces)
            {
                var shiftsOnDayAtWorkplace = schedulingParameters.Employees
                    .Select(e => new ShiftAssignment(e, day, workplace))
                    .ToArray();

                engine.AddSumConstraint(shiftsOnDayAtWorkplace, Constraint.Eq, 1);
            }
        }
    }

    public static void EveryEmployeeHasOneShiftAtATime(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var employee in schedulingParameters.Employees)
        {
            foreach (var day in schedulingParameters.Days)
            {
                var employeesShiftsThatDay = schedulingParameters.Workplaces
                    .Select(w => new ShiftAssignment(employee, day, w))
                    .ToArray();
 
                engine.AddSumConstraint(employeesShiftsThatDay, Constraint.Lte, 1);
            }
        }
    }

    public static void NoConsecutiveShiftsForEmployee(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var employee in schedulingParameters.Employees)
        {
            foreach (var (day1, day2) in schedulingParameters.Days.Zip(schedulingParameters.Days.Skip(1)))
            {
                var employeesShiftsInTwoDays = schedulingParameters.Workplaces
                    .SelectMany(w => new [] {new ShiftAssignment(employee, day1, w), new ShiftAssignment(employee, day2, w)})
                    .ToArray();
 
                engine.AddSumConstraint(employeesShiftsInTwoDays, Constraint.Lte, 1);
            }
        }
    }

    public static void NoAlternativeDayShiftsForEmployee(SolverParameters schedulingParameters, ISchedulingEngine engine)
    {
        foreach (var employee in schedulingParameters.Employees)
        {
            foreach (var (day1, day3) in schedulingParameters.Days.Zip(schedulingParameters.Days.Skip(2)))
            {
                if (day1.Date.DayOfWeek == DayOfWeek.Friday)
                {
                    continue;
                }
                
                var employeesShiftsInAlternateDays = schedulingParameters.Workplaces
                    .SelectMany(w => new [] {new ShiftAssignment(employee, day1, w), new ShiftAssignment(employee, day3, w)})
                    .ToArray();
 
                engine.AddSumConstraint(employeesShiftsInAlternateDays, Constraint.Lte, 1);
            }
        }
        
    }


    public static void EveryCompetenceGroupMustHaveAtLeastOneLevel1Employee(SchedulingParameters schedulingParameters, IReadOnlyCollection<Day> days, ISchedulingEngine engine)
    {
        foreach (var competenceGroup in schedulingParameters.CompetenceGroups)
        {
            foreach (var day in days)
            {
                IReadOnlyCollection<ShiftAssignment> employeesLevel1AndWorkplacesCombinations = schedulingParameters.Employees
                    .Where(e => e.CompetenceLevel == CompetenceLevel.Level1)
                    .SelectMany(e => competenceGroup.Workplaces.Select(w => new ShiftAssignment(e, day, w)))
                    .ToArray();
                
                engine.AddSumConstraint(employeesLevel1AndWorkplacesCombinations, Constraint.Gte, 1);
            }
        }
    }

    public static void DisallowShiftsWithoutEmployeeCapability(SolverParameters solverParameters, ISchedulingEngine engine)
    {
        foreach (var employee in solverParameters.Employees)
        {
            var capableWorkplaces = employee.Capabilities.WorkplaceCapabilities
                .OfType<WorkplaceCapability.Green>()
                .Select(wc => wc.Workplace);
            var incapableWorkplaces = solverParameters.Workplaces.Except(capableWorkplaces);

            foreach (var incapableWorkplace in incapableWorkplaces)
            {
                foreach (var day in solverParameters.Days)
                {
                    engine.AddVariableConstraint(new ShiftAssignment(employee, day, incapableWorkplace), Constraint.Eq, 0);
                }
            }
        }
    }
}
