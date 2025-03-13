using Godot;

namespace MafiaHostAssistant;

public sealed partial class OP_HighlightPlayer : Operation
{
	[Export] private Label varNameLabel;
	[Export] private TextureRect varTypeTextureRect;

	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler variableHandler;

    protected override void OnAddition(BehaviorEditor behaviorEditor)
    {
        this.behaviorEditor = behaviorEditor;
		variableHandler = new(behaviorEditor, this, Tr("TK:VARTYPE_PLAYER"), BehaviorVariableType.Player, varNameLabel, varTypeTextureRect);
    }

    public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.HighlightPlayer, new Arguments(variableHandler.Variable.TrueVariableName));
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		variableHandler.RegisterVariable(ParentScope.FindVariableByName(((Arguments)argumens).varName));
	}

    public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_HIGHLIGHT-PLAYER");
	}

    public override bool IsStateless => true;

    public class Arguments : OperationReference.Arguments
	{
		public string varName;
		public Arguments(string varName)
		{
			this.varName = varName;
		}
	}
}
