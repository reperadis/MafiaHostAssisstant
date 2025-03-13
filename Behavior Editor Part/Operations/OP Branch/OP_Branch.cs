using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_Branch : OPMessagingOperation
{
    [Export] private PackedScene branchScene;

    protected override void OnAddition(BehaviorEditor behaviorEditor)
    {
        base.OnAddition(behaviorEditor);
        this.behaviorEditor = behaviorEditor;
    }

    public void OpenConfigWindow()
    {
        behaviorEditor.SetConfigWindowActive();
        CreateMessageField();
    }

    public void AddBranch()
    {
        OP_Branch_Branch branch = branchScene.Instantiate<OP_Branch_Branch>();
        branch.SetUp(behaviorEditor, ParentScope, this);
        AddChild(branch);
        MoveChild(branch, -2); // To be above the add button
    }

    public override bool IsStateless => true;

    public override OperationReference GetOperationReference()
    {
        List<BranchArguments> branches = new();
        foreach (Node node in GetChildren())
        {
            if (node is OP_Branch_Branch branch)
            {
                branches.Add(branch.GetBranchReference());
            }
        }
        return new(OperationName.Branch, new Arguments(BEToShared(message), branches));
    }

    public override string GetReadableOpearationName()
    {
        return Tr("TK:OP_BRANCH");
    }

    public override void Write(OperationReference.Arguments argumens)
    {
        Arguments args = (Arguments)argumens;
        WriteMessage(args.message);

        foreach (BranchArguments branch in args.branches)
        {
            AddBranch();
            OP_Branch_Branch branchNode = GetChild<OP_Branch_Branch>(-2);
            branchNode.Write(branch);
        }
    }

    public class Arguments : OperationReference.Arguments
    {
        public List<SharedDynamicStringElementData> message;
        public List<BranchArguments> branches;

        public Arguments(List<SharedDynamicStringElementData> message, List<BranchArguments> branches)
        {
            this.message = message;
            this.branches = branches;
        }
    }

    public class BranchArguments
    {
        public string branchLabel;
        public List<OperationReference> sequence;

        public BranchArguments(string branchLabel, List<OperationReference> sequence)
        {
            this.branchLabel = branchLabel;
            this.sequence = sequence;
        }
    }
}
