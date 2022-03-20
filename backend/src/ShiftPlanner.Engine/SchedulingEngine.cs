using Google.OrTools.Sat;
using ShiftPlanner.Domain;

namespace ShiftPlanner.Engine;

internal class SchedulingEngine
{
    private readonly IModel _model;
    private ISolver _solver;

    public SchedulingEngine(IModel model, ISolver solver)
    {
        _model = model;
        _solver = solver;
    }

    public void CalculateSchedule(SchedulingParameters input)
    {
        var employees = CreateIndex(input.Employees);
        var workplaces = CreateIndex(input.Workplaces);
        var days = CreateDays(input.Month);

        foreach (var e in employees.Values)
        {
            foreach (var d in days.Select(d => d.Date.Day))
            {
                foreach (var w in workplaces.Values)
                {
                    _model.NewBoolVar(VariableName.Schedule(e, d, w));
                }
            }
        }
    }

    private static IDictionary<T, int> CreateIndex<T>(IReadOnlyCollection<T> elements)
        where T : notnull
    {
        return elements.Select(KeyValuePair.Create).ToDictionary(x => x.Key, y => y.Value + 1);
    }

    private static IReadOnlyCollection<Day> CreateDays(ScheduledMonth month)
    {
        return Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(d => new Day(new DateOnly(month.Year, month.Month, d)))
            .ToArray();
    }
}
