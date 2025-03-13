using Godot;

namespace MafiaHostAssistant;

public partial class OP_Branch_Branch : Control
{
    [Export] private Label branchLabelLabel;
    [Export] private OperationScope behaviorScope;

    private string branchLabel;
    private BehaviorEditor behaviorEditor;

    public void SetUp(BehaviorEditor behaviorEditor, OperationScope parentScope, Operation parentOperation)
    {
        this.behaviorEditor = behaviorEditor;
        behaviorScope.SetUp(parentScope, parentOperation, parentScope.MustExitWithBool, parentScope.RootEntryPoint, behaviorEditor);
    }

    public void OpenConfigWindow()
    {
        behaviorEditor.SetConfigWindowActive();
        behaviorEditor.CreateStringField(Tr("TK:OP_FIELD_BRANCH-LABEL"), branchLabel, ReceiveBranchLabel);
    }

    private void ReceiveBranchLabel(string label)
    {
        branchLabel = label;
        branchLabelLabel.Text = label;
    }

    public void Write(OP_Branch.BranchArguments arguments)
    {
        ReceiveBranchLabel(arguments.branchLabel);
        behaviorScope.WriteScope(arguments.sequence);
    }

    public bool GetIsStateless()
    {
        return behaviorScope.IsStateless();
    }

    public OP_Branch.BranchArguments GetBranchReference()
    {
        return new(branchLabel, behaviorScope.ReadScope());
    }
}
