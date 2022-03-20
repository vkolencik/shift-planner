using Google.OrTools.Sat;

namespace ShiftPlanner.Engine.GoogleCp;

internal class GoogleCpModel : IModel
{
    private CpModel _model;

    public GoogleCpModel()
    {
        _model = new CpModel();
    }

    public void NewBoolVar(VariableName variableName)
    {
        _model.NewBoolVar(variableName.Value);
    }
}
