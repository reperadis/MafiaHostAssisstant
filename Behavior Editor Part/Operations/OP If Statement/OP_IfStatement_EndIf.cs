using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public sealed partial class OP_IfStatement_EndIf : Control
{
	[Export] private Label varNameLabel; // Else If Case
	[Export] private TextureRect varTypeTextureRect; // Else If Case
	[Export] private OperationScope behaviorScope;

	[Export] private Control fullEndState;
	[Export] private Control elseState;
	[Export] private Control elseIfState;

	private OP_IfStatement ifStatement;
	public OP_IfStatement_EndIf upperEndIf;
	public OP_IfStatement_EndIf lowerEndIf;
	public bool isElseIf;

	private OperationScope parentScope;
	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler elseIfCaseVariableHandler;

	public void SetUp(OperationScope parentScope, OP_IfStatement ifStatement, OP_IfStatement_EndIf upperEndIf, BehaviorEditor behaviorEditor)
	{
		this.parentScope = parentScope;
		this.ifStatement = ifStatement;
		this.upperEndIf = upperEndIf;
		this.behaviorEditor = behaviorEditor;
		behaviorScope.SetUp(parentScope, ifStatement, parentScope.MustExitWithBool, parentScope.RootEntryPoint, behaviorEditor);
		elseIfCaseVariableHandler = new(behaviorEditor, ifStatement, Tr("TK:OP_FIELD_CONDITION"), BehaviorVariableType.Bool, varNameLabel, varTypeTextureRect);
	}

	public void OpenConfigWindow() // ElseIf case
	{
		behaviorEditor.SetConfigWindowActive();
		elseIfCaseVariableHandler.CreateSelectionField(false);
	}

	public void SwitchToElse()
	{
		fullEndState.Visible = false;
		elseState.Visible = true;
		behaviorScope.Visible = true;
		lowerEndIf = ifStatement.endIfScene.Instantiate<OP_IfStatement_EndIf>();
		ifStatement.AddChild(lowerEndIf);
		lowerEndIf.SetUp(parentScope, ifStatement, this, behaviorEditor);
	}

	public void SwitchToElseIf()
	{
		elseIfState.Visible = false;
		if (upperEndIf == null || upperEndIf.isElseIf)
		{
			elseIfState.Visible = true;
		}
		else
		{
			SwitchToFullEnd();
		}
		isElseIf = true;
	}

	public void SwitchToFullEnd()
	{
		if (!CanSwitchToFullEndRepeating())
		{
			return;
		}
		behaviorScope.Visible = false;
		fullEndState.Visible = true;
		lowerEndIf.QueueFree();
		lowerEndIf = null;
		isElseIf = false;
	}

	public bool CanSwitchToFullEndRepeating() // From upper to lower
	{
		if (behaviorScope.GetChildCount() != 0)
		{
			return false;
		}
		if (lowerEndIf != null)
		{
			return lowerEndIf.CanSwitchToFullEndRepeating();
		}
		return true;
	}

	public void DeleteRepeating()
	{
		foreach (Node node in behaviorScope.GetChildren())
		{
			if (node is Operation operation)
			{
				operation.Delete();
			}
		}
		lowerEndIf?.DeleteRepeating();
	}

	public OP_IfStatement.EndIfArguments GetEndIfArguments()
	{
		List<OperationReference> sequence = behaviorScope.ReadScope();
		byte state;
		if (fullEndState.Visible)
		{
			state = 0;
		}
		else if (elseState.Visible)
		{
			state = 1;
		}
		else
		{
			state = 2;
		}
		return new OP_IfStatement.EndIfArguments(state, sequence, elseIfCaseVariableHandler.Variable.TrueVariableName);
	}

	public void WriteRecursive(List<OP_IfStatement.EndIfArguments> arguments, int index)
	{
		OP_IfStatement.EndIfArguments args = arguments[index];

		if (args.state == 1)
		{
			SwitchToElse();
			behaviorScope.WriteScope(args.sequence);
			lowerEndIf.WriteRecursive(arguments, index + 1);
			return;
		}
		if (args.state == 2)
		{
			SwitchToElseIf();
			behaviorScope.WriteScope(args.sequence);
			elseIfCaseVariableHandler.RegisterVariable(parentScope.FindVariableByName(args.elseIfCaseVarName));
			lowerEndIf.WriteRecursive(arguments, index + 1);
		}
	}

	public bool GetIsStateless()
	{
		return behaviorScope.IsStateless();
	}
}
