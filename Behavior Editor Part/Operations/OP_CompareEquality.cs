using Godot;

namespace MafiaHostAssistant;

public partial class OP_CompareEquality : Operation
{
	[Export] private Label assignToVarNameLabel;
	[Export] private TextureRect assignToVarTypeTextureRect;
	[Export] private Label leftVarNameLabel;
	[Export] private TextureRect leftVarTypeTextureRect;
	[Export] private Label rightVarNameLabel;
	[Export] private TextureRect rightVarTypeTextureRect;
	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler leftVariableHandler;
	private BehaviorVariableHandler rightVariableHandler;
	private BehaviorVariableHandler assignToVariableHandler;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		leftVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_LEFT"), BehaviorVariableType.Anything, leftVarNameLabel, leftVarTypeTextureRect);
		leftVariableHandler.PostVariableRegistered += OnPostLeftVariableRegistered;
		rightVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_RIGHT"), BehaviorVariableType.Nothing, rightVarNameLabel, rightVarTypeTextureRect);
		assignToVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Bool, assignToVarNameLabel, assignToVarTypeTextureRect);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		assignToVariableHandler.CreateSelectionField(true);
		leftVariableHandler.CreateSelectionField(false);
		if (leftVariableHandler.badVariableErrorIndex == -1)
		{
			rightVariableHandler.CreateSelectionField(false);
		}
	}
	
	private void OnPostLeftVariableRegistered()
	{
		rightVariableHandler.ChangeExpectedType(leftVariableHandler.Variable.VariableType);
		rightVariableHandler.CreateSelectionField(false);
	}
	
	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.CompareEquality, new Arguments(leftVariableHandler.Variable.TrueVariableName, rightVariableHandler.Variable.TrueVariableName, assignToVariableHandler.Variable.TrueVariableName));
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_COMPARE-EQUALITY");
	}

	public override bool IsStateless
	{
		get => true;
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		leftVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.leftVarName));
		rightVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.rightVarName));
		assignToVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.assignToVarName));
	}

	public class Arguments : OperationReference.Arguments
	{
		public string leftVarName;
		public string rightVarName;
		public string assignToVarName;

		public Arguments(string leftVarName, string rightVarName, string assignToVarName)
		{
			this.leftVarName = leftVarName;
			this.rightVarName = rightVarName;
			this.assignToVarName = assignToVarName;
		}
	}
}
