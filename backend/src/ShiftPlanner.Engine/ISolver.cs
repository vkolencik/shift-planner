namespace ShiftPlanner.Engine;

internal interface ISolver
{
    void NewBoolVar(VariableName variableName);

    void Solve();

    long Result(VariableName variableName);
}
