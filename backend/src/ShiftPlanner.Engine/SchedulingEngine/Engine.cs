using ShiftPlanner.Domain;
using ShiftPlanner.Engine.SchedulingEngine.GoogleCp;

namespace ShiftPlanner.Engine.SchedulingEngine
{
    internal class Engine : ISchedulingEngine
    {
        private readonly ISolver _solver;
        private IDictionary<Employee, int> _employees = null!;
        private IDictionary<Workplace, int> _workplaces = null!;
        private IDictionary<Day, int> _days = null!;

        internal Engine(ISolver solver, SolverParameters input)
        {
            _solver = solver;
            Init(input);
        }

        public static Engine CreateGoogleSolverEngine(SolverParameters input) =>
            new Engine(new GoogleCpSolver(), input);

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

        public Schedule? Solve()
        {
            _solver.Solve();

            if (_solver.SolverStatus == SolverStatus.NotFound)
            {
                return null;
            }

            var outputModel = new List<ShiftAssignment>();

            foreach (var day in _days)
            {
                foreach (var workplace in _workplaces)
                {
                    var assignedEmployee = _employees
                        .Single(e => _solver.Result(VariableName.Schedule(e.Value, day.Value, workplace.Value)) == 1)
                        .Key;

                    outputModel.Add(new ShiftAssignment(assignedEmployee, day.Key, workplace.Key));
                }
            }

            return Schedule.CreateSchedule(outputModel);
        }

        private static IDictionary<T, int> CreateIndex<T>(IReadOnlyCollection<T> elements)
            where T : notnull
        {
            return elements.Select(KeyValuePair.Create).ToDictionary(x => x.Key, y => y.Value + 1);
        }

        public void AddSumConstraint(IReadOnlyCollection<ShiftAssignment> shifts, Constraint constraint, uint bound)
        {
            var shiftVariables = shifts
                .Select(s => VariableName.Schedule(_employees[s.Employee], _days[s.Shift.Day], _workplaces[s.Shift.Workplace]))
                .ToArray();
            
            _solver.AddSumConstraint(shiftVariables, constraint, bound);
        }

        public void AddVariableConstraint(ShiftAssignment shift, Constraint constraint, int value)
        {
            _solver.AddVariableConstraint(
                VariableName.Schedule(_employees[shift.Employee], _days[shift.Shift.Day], _workplaces[shift.Shift.Workplace]),
                constraint,
                value);
        }
    }
}
