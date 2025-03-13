using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_CompareIntegers : Operation
{
	[Export] private Label leftVarNameLabel;
	[Export] private TextureRect leftVarTypeTextureRect;
	[Export] private Label rightVarNameLabel;
	[Export] private TextureRect rightVarTypeTextureRect;
	[Export] private Label assignToVarNameLabel;
	[Export] private TextureRect assignToVarTypeTextureRect;
	[Export] private Label operationLabel;
	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler leftVariableHandler;
	private BehaviorVariableHandler rightVariableHandler;
	private BehaviorVariableHandler assignToVariableHandler;
	private OperationType operationType;
	private List<Dropdown.ElementData> operationOptions;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		leftVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_LEFT"), BehaviorVariableType.Integer, leftVarNameLabel, leftVarTypeTextureRect);
		rightVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_RIGHT"), BehaviorVariableType.Integer, rightVarNameLabel, rightVarTypeTextureRect);
		assignToVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Bool, assignToVarNameLabel, assignToVarTypeTextureRect);
		operationOptions = new()
		{
			new Dropdown.ElementData(null, ">"),
			new Dropdown.ElementData(null, "<"),
			new Dropdown.ElementData(null, "=="),
			new Dropdown.ElementData(null, "!="),
			new Dropdown.ElementData(null, ">="),
			new Dropdown.ElementData(null, "<=")
		};
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		assignToVariableHandler.CreateSelectionField(true);
		rightVariableHandler.CreateSelectionField(false);
		leftVariableHandler.CreateSelectionField(false);
		behaviorEditor.CreateEnumField(Tr("TK:OP_FIELD_OPERATION"), operationOptions, (int)operationType, ReceiveOperationIndex);
	}
	
	private void ReceiveOperationIndex(int index)
	{
		operationType = (OperationType)index;
		operationLabel.Text = operationOptions[index].label;
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.CompareIntegers, new Arguments(leftVariableHandler.Variable.TrueVariableName, rightVariableHandler.Variable.TrueVariableName, assignToVariableHandler.Variable.TrueVariableName, operationType));
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_COMPARE-INTEGERS");
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
		operationType = args.operationType;
		operationLabel.Text = operationOptions[(int)operationType].label;
	}
	
	public class Arguments : OperationReference.Arguments
	{
		public string leftVarName;
		public string rightVarName;
		public string assignToVarName;
		public OperationType operationType;
		
		public Arguments(string leftVarName, string rightVarName, string assignToVarName, OperationType operationType)
		{
			this.leftVarName = leftVarName;
			this.rightVarName = rightVarName;
			this.assignToVarName = assignToVarName;
			this.operationType = operationType;
		}
	}

	public enum OperationType
	{
		GreaterThan,
		LessThan,
		Equal,
		NotEqual,
		GreaterThanOrEqual,
		LessThanOrEqual
	}
}
