using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_IfStatement : Operation
{
	[Export] private Label varNameLabel;
	[Export] private TextureRect varTypeTextureRect;
	[Export] private OP_IfStatement_EndIf endIf;
	[Export] private OperationScope behaviorScope;

	[Export] public PackedScene endIfScene; // To be accessed by EndIfs, Godot does not like circular dependencies

	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler variableHandler;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		endIf.SetUp(ParentScope, this, null, behaviorEditor);
		behaviorScope.SetUp(ParentScope, this, ParentScope.MustExitWithBool, ParentScope.RootEntryPoint, behaviorEditor);
		variableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_CONDITION"), BehaviorVariableType.Bool, varNameLabel, varTypeTextureRect);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		variableHandler.CreateSelectionField(false);
	}

	protected override void OnDeletion()
	{
		foreach (Node node in behaviorScope.GetChildren())
		{
			if (node is Operation operation)
			{
				operation.Delete();
			}
		}
		endIf.DeleteRepeating();
	}

	public override OperationReference GetOperationReference()
	{
		List<EndIfArguments> endIfVals = new();
		OP_IfStatement_EndIf current = endIf;
		while (current != null)
		{
			endIfVals.Add(current.GetEndIfArguments());
			current = endIf.lowerEndIf;
		}
		return new OperationReference(OperationName.IfStatement, new Arguments(variableHandler.Variable.TrueVariableName, behaviorScope.ReadScope(), endIfVals));
	}
	

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;

		variableHandler.RegisterVariable(ParentScope.FindVariableByName(args.variableName));

		behaviorScope.WriteScope(args.ifSequence);

		endIf.WriteRecursive(args.endIfs, 0);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_IF-STATEMENT");
	}

	public override bool IsStateless
	{
		get
		{
			if (!behaviorScope.IsStateless())
			{
				return false;
			}
			OP_IfStatement_EndIf currentEndIf = endIf;
			while (currentEndIf != null)
			{
				if (!currentEndIf.GetIsStateless())
				{
					return false;
				}
				currentEndIf = currentEndIf.lowerEndIf;
			}
			return true;
		}
	}

	[Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public string variableName;
		public List<OperationReference> ifSequence;
		public List<EndIfArguments> endIfs;

		public Arguments(string variableName, List<OperationReference> ifSequence, List<EndIfArguments> endIfs)
		{
			this.variableName = variableName;
			this.ifSequence = ifSequence;
			this.endIfs = endIfs;
		}
	}

	public class EndIfArguments
	{
		public byte state; // 0: full end, 1: else, 2: else if
		public List<OperationReference> sequence;
		public string elseIfCaseVarName;

		public EndIfArguments(byte state, List<OperationReference> sequence, string elseIfCaseVarName)
		{
			this.state = state;
			this.sequence = sequence;
			this.elseIfCaseVarName = elseIfCaseVarName;
		}
	}
}
