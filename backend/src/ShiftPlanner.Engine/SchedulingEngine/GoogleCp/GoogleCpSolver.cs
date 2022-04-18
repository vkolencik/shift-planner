using System.ComponentModel;
using Google.OrTools.Sat;

namespace ShiftPlanner.Engine.SchedulingEngine.GoogleCp;

internal class GoogleCpSolver : ISolver
{
    private CpModel _model;
    private CpSolver _solver;
    private readonly IDictionary<VariableName, IntVar> _variables = new Dictionary<VariableName, IntVar>();
    private CpSolverStatus? _status;

    public GoogleCpSolver()
    {
        _model = new CpModel();
        _solver = new CpSolver();
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
        _status = _solver.Solve(_model);
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

    public void AddSumConstraint(IReadOnlyCollection<VariableName> variableNames, Constraint constraint, int bound)
    {
        var variables = variableNames.Select(v => _variables[v]).ToArray();
        switch (constraint)
        {
            case Constraint.Eq:
                _model.Add(LinearExpr.Sum(variables) == bound);
                break;
            case Constraint.Gte:
                _model.Add(LinearExpr.Sum(variables) >= bound);
                break;
            case Constraint.Lte:
                _model.Add(LinearExpr.Sum(variables) <= bound);
                break;
            default: throw new InvalidEnumArgumentException(nameof(constraint), (int) constraint, typeof(Constraint));                
        }
    }
}
