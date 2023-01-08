using System.ComponentModel;
using Google.OrTools.Sat;
using RConstraint = ShiftPlanner.Rules.Constraint;

namespace ShiftPlanner.Engine.SchedulingEngine.GoogleCp;

internal class GoogleCpSolver : ISolver
{
    private readonly CpModel _model;
    private readonly CpSolver _solver;
    private readonly IDictionary<VariableName, IntVar> _variables = new Dictionary<VariableName, IntVar>();
    private CpSolverStatus? _status;

    public GoogleCpSolver()
    {
        _model = new CpModel();
        _solver = new CpSolver();
        _solver.StringParameters = "enumerate_all_solutions:true";
    }

    public void NewBoolVar(VariableName variableName)
    {
        if (_variables.ContainsKey(variableName))
        {
            throw new ArgumentException($"Variable {variableName.Value} was already created", nameof(variableName));
        }

        var solverVariable = _model.NewBoolVar(variableName.Value);

        _variables.Add(variableName, solverVariable);
    }

    public void Solve()
    {
        _status = _solver.Solve(_model, new LimitSolutionCallback(1));
    }

    public long Result(VariableName variableName)
    {
        if (_status is null)
        {
            throw new InvalidOperationException("Solver hasn't been executed yet. Call the .Solve() method first.");
        }

        if (_status != CpSolverStatus.Optimal && _status != CpSolverStatus.Feasible)
        {
            throw new InvalidOperationException($"Solver didn't find solution (solution status: {_status.ToString()}).");
        }

        if (!_variables.TryGetValue(variableName, out var value))
        {
            throw new ArgumentException($"Variable {variableName.Value} not found in model", nameof(variableName));
        }

        return _solver.Value(value);
    }

    public void AddSumConstraint(IReadOnlyCollection<VariableName> variableNames, RConstraint constraint, uint bound)
    {
        var variables = variableNames.Select(v => _variables[v]).ToArray();
        switch (constraint)
        {
            case RConstraint.Eq:
                _model.Add(LinearExpr.Sum(variables) == bound);
                break;
            case RConstraint.Gte:
                _model.Add(LinearExpr.Sum(variables) >= bound);
                break;
            case RConstraint.Lte:
                _model.Add(LinearExpr.Sum(variables) <= bound);
                break;
            default: throw new InvalidEnumArgumentException(nameof(constraint), (int) constraint, typeof(Constraint));
        }
    }

    public void AddVariableConstraint(VariableName variableName, RConstraint constraint, int value)
    {
        var variable = _variables[variableName];
        switch (constraint)
        {
            case RConstraint.Eq:
                _model.Add(variable == value);
                break;
            case RConstraint.Gte:
                _model.Add(variable >= value);
                break;
            case RConstraint.Lte:
                _model.Add(variable <= value);
                break;
            default: throw new InvalidEnumArgumentException(nameof(constraint), (int) constraint, typeof(Constraint));
        }
    }

    public void AddSumRelationConstraint(IReadOnlyCollection<VariableName> variableNames1, RConstraint constraint, IReadOnlyCollection<VariableName> variableNames2)
    {
        var variables1 = variableNames1.Select(v => _variables[v]).ToArray();
        var variables2 = variableNames2.Select(v => _variables[v]).ToArray();

        switch (constraint)
        {
            case RConstraint.Eq:
                _model.Add(LinearExpr.Sum(variables1) == LinearExpr.Sum(variables2));
                break;
            case RConstraint.Gte:
                _model.Add(LinearExpr.Sum(variables1) >= LinearExpr.Sum(variables2));
                break;
            case RConstraint.Lte:
                _model.Add(LinearExpr.Sum(variables1) <= LinearExpr.Sum(variables2));
                break;
            default: throw new InvalidEnumArgumentException(nameof(constraint), (int) constraint, typeof(Constraint));
        }
    }

    public SolverStatus SolverStatus => _solver.Response.Status switch
    {
        CpSolverStatus.Unknown => SolverStatus.Initialized,
        CpSolverStatus.Infeasible => SolverStatus.NotFound,
        CpSolverStatus.Feasible => SolverStatus.Success,
        CpSolverStatus.Optimal => SolverStatus.Success,
        _ => throw new NotSupportedException($"Unsupported solver status {_solver.Response.Status}")
    };

    class LimitSolutionCallback : CpSolverSolutionCallback
    {
        private int solutionCount = 0;
        private readonly int _solutionLimit;

        public LimitSolutionCallback(int solutionLimit)
        {
            _solutionLimit = solutionLimit;
        }

        public override void OnSolutionCallback()
        {
            solutionCount++;
            Console.WriteLine("Solution found");
            if (solutionCount >= _solutionLimit)
            {
                Console.WriteLine($"Stop search after {_solutionLimit} solutions");
                StopSearch();
            }
        }
   
    }
}

