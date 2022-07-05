namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISolver
{
    void NewBoolVar(VariableName variableName);
    void Solve();
    long Result(VariableName variableName);
    void AddSumConstraint(IReadOnlyCollection<VariableName> variableNames, Constraint constraint, uint bound);
    void AddVariableConstraint(VariableName variableName, Constraint constraint, int value);
    void AddRelationConstraint(VariableName variableName1, Constraint constraint, VariableName variableName2);
    SolverStatus SolverStatus { get; }
}
