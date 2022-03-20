using System.Collections.Immutable;
using Google.OrTools.Sat;
using ShiftPlanner.Domain;
using ShiftPlanner.Engine.GoogleCp;

namespace ShiftPlanner.Engine
{
    internal class SchedulingEngine
    {
        private readonly IModel _model;
        private ISolver _solver;

        internal SchedulingEngine(IModel model, ISolver solver)
        {
            _model = model;
            _solver = solver;
        }

        public static SchedulingEngine CreateGoogleSolverEngine() => new SchedulingEngine(new GoogleCpModel(), new GoogleCpSolver()); 
    
        public Schedule CalculateSchedule(SchedulingParameters input)
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

            return new Schedule(ImmutableDictionary<DateOnly, IDictionary<Workplace, Employee>>.Empty);
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
}
