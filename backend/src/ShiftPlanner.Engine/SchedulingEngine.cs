using ShiftPlanner.Domain;
using ShiftPlanner.Engine.GoogleCp;

namespace ShiftPlanner.Engine
{
    internal class SchedulingEngine
    {
        private readonly ISolver _solver;
        private IDictionary<Employee, int> _employees;
        private IDictionary<Workplace, int> _workplaces;
        private IReadOnlyCollection<Day> _days;

        internal SchedulingEngine(ISolver solver, SchedulingParameters input)
        {
            _solver = solver;
            Init(input);
        }

        public static SchedulingEngine CreateGoogleSolverEngine(SchedulingParameters input) =>
            new SchedulingEngine(new GoogleCpSolver(), input);

        private void Init(SchedulingParameters input)
        {
            _employees = CreateIndex(input.Employees);
            _workplaces = CreateIndex(input.Workplaces);
            _days = input.Days;

            foreach (var e in _employees.Values)
            {
                foreach (int d in Enumerable.Range(1, _days.Count))
                {
                    foreach (var w in _workplaces.Values)
                    {
                        _solver.NewBoolVar(VariableName.Schedule(e, d, w));
                    }
                }
            }
        }

        public Schedule Solve()
        {
            _solver.Solve();

            var outputModel = new List<Shift>();

            foreach (int d in Enumerable.Range(1, _days.Count))
            {
                foreach (var workplace in _workplaces)
                {
                    var assignedEmployee = _employees
                        .Single(e => _solver.Result(VariableName.Schedule(e.Value, d, workplace.Value)) == 1)
                        .Key;

                    outputModel.Add(new Shift(assignedEmployee, _days.ElementAt(d - 1).Date, workplace.Key));
                }
            }

            return Schedule.CreateSchedule(outputModel);
        }

        private static IDictionary<T, int> CreateIndex<T>(IReadOnlyCollection<T> elements)
            where T : notnull
        {
            return elements.Select(KeyValuePair.Create).ToDictionary(x => x.Key, y => y.Value + 1);
        }
    }
}
