using ShiftPlanner.Domain;
using ShiftPlanner.Engine.GoogleCp;

namespace ShiftPlanner.Engine
{
    internal class SchedulingEngine
    {
        private readonly ISolver _solver;
        private IDictionary<Employee, int> _employees = null!;
        private IDictionary<Workplace, int> _workplaces = null!;
        private IDictionary<Day, int> _days = null!;

        internal SchedulingEngine(ISolver solver, SolverParameters input)
        {
            _solver = solver;
            Init(input);
        }

        public static SchedulingEngine CreateGoogleSolverEngine(SolverParameters input) =>
            new SchedulingEngine(new GoogleCpSolver(), input);

        private void Init(SolverParameters input)
        {
            _employees = CreateIndex(input.Employees);
            _workplaces = CreateIndex(input.Workplaces);
            _days = CreateIndex(input.Days);

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

            foreach (var day in _days)
            {
                foreach (var workplace in _workplaces)
                {
                    var assignedEmployee = _employees
                        .Single(e => _solver.Result(VariableName.Schedule(e.Value, day.Value, workplace.Value)) == 1)
                        .Key;

                    outputModel.Add(new Shift(assignedEmployee, day.Key, workplace.Key));
                }
            }

            return Schedule.CreateSchedule(outputModel);
        }

        private static IDictionary<T, int> CreateIndex<T>(IReadOnlyCollection<T> elements)
            where T : notnull
        {
            return elements.Select(KeyValuePair.Create).ToDictionary(x => x.Key, y => y.Value + 1);
        }

        public void AddSumConstraint(IReadOnlyCollection<Shift> shifts, Constraint constraint, int bound)
        {
            var shiftVariables = shifts
                .Select(s => VariableName.Schedule(_employees[s.Employee], _days[s.Day], _workplaces[s.Workplace]))
                .ToArray();
            
            _solver.AddSumConstraint(shiftVariables, constraint, bound);
        }
    }
}
