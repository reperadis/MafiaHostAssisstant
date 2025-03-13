using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_LogicalOperation : Operation
{
	[Export] private Label assignToVarNameLabel;
	[Export] private TextureRect assignToVarTypeTextureRect;
	[Export] private Label leftVarNameLabel;
	[Export] private TextureRect leftVarTypeTextureRect;
	[Export] private Label operationNameLabel;
	[Export] private TextureRect operationIconTextureRect; // TODO: All boolean operations have an icon "=>" for implication, "v" for disjunction, etc.
	[Export] private Label rightVarNameLabel;
	[Export] private TextureRect rightVarTypeTextureRect;

	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler assignToVariableHandler;
	private BehaviorVariableHandler leftVariableHandler;
	private BehaviorVariableHandler rightVariableHandler;
	private OperationType operationType;
	private List<Dropdown.ElementData> operationOptions;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		operationOptions = new()
		{
			// TODO: populate this;
		};
		assignToVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Bool, assignToVarNameLabel, assignToVarTypeTextureRect);
		leftVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_LEFT"), BehaviorVariableType.Bool, leftVarNameLabel, leftVarTypeTextureRect);
		rightVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_RIGHT"), BehaviorVariableType.Bool, rightVarNameLabel, rightVarTypeTextureRect);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		assignToVariableHandler.CreateSelectionField(true);
		leftVariableHandler.CreateSelectionField(false);
		rightVariableHandler.CreateSelectionField(false);
		behaviorEditor.CreateEnumField(Tr("TK:OP_FIELD_OPERATION"), operationOptions, (int)operationType, RegisterOperation);
	}

	private void RegisterOperation(int index)
	{
		operationNameLabel.Text = operationOptions[index].label;
		operationIconTextureRect.Texture = operationOptions[index].icon;
		operationType = (OperationType)index;
	}

	public override bool IsStateless
	{
		get => true;
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.LogicalOperation, new Arguments(assignToVariableHandler.Variable.TrueVariableName, leftVariableHandler.Variable.TrueVariableName, rightVariableHandler.Variable.TrueVariableName, operationType));
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_LOGICAL-OPERATION");
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		assignToVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.assignToVarName));
		leftVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.leftVarName));
		rightVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.rightVarName));
		RegisterOperation((int)args.operationType);
	}

	public class Arguments : OperationReference.Arguments
	{
		public string assignToVarName;
		public string leftVarName;
		public string rightVarName;
		public OperationType operationType;

		public Arguments(string assignToVarName, string leftVarName, string rightVarName, OperationType operationType)
		{
			this.assignToVarName = assignToVarName;
			this.leftVarName = leftVarName;
			this.rightVarName = rightVarName;
			this.operationType = operationType;
		}
	}

	public enum OperationType
	{
		Conjunction,
		Disjunction,
		ExclusiveDisjunction,
		Implication,
		ConverseImplication,
		Equivalence,
		NonConjunction,
		NonDisjunction,
		NonImplication,
		ConverseNonImplication,
	}
}
