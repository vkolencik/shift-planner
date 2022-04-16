namespace ShiftPlanner.Engine.SchedulingEngine;

internal interface ISolver
{
    void NewBoolVar(VariableName variableName);
    void Solve();
    long Result(VariableName variableName);
    void AddSumConstraint(IReadOnlyCollection<VariableName> variableNames, Constraint constraint, int bound);
}
