using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public sealed partial class OperationScope : Control
{
	private BehaviorEditor behaviorEditor;
	
	public Operation ParentOperation { get; private set; }
    public EntryPoint RootEntryPoint { get; private set; }
    public OperationScope ParentCodeScope { get; private set; }

    public readonly List<BehaviorVariable> variables = new();
	public bool MustExitWithBool { get; private set; }
	public Action<BehaviorVariable> OnVariableAddedOrRenamed;
	public int IdentationLevel { get; private set; }
	public int CycleLevel { get; private set; }

    public void SetUp(OperationScope parentScope, Operation parentOperation, bool mustExitWithBool, EntryPoint entryPoint, BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		ParentCodeScope = parentScope;
		ParentOperation = parentOperation;
		MustExitWithBool = mustExitWithBool;
		RootEntryPoint = entryPoint;
		if (ParentCodeScope != null)
		{
			ParentCodeScope.OnVariableAddedOrRenamed += OnVariableAddedOrRenamed;
			IdentationLevel = ParentCodeScope.IdentationLevel + 1;
			CycleLevel = ParentCodeScope.CycleLevel;
			if (GetParent() is OP_ForeachLoop) 
			{
				CycleLevel++;
			}
		}
		else
		{
			behaviorEditor.OnVariableAddedOrRenamed += OnVariableAddedOrRenamed;
		}
	}

	public BehaviorVariable FindVariableByName(string variableName) // Recursively. Returns null if not found
	{
		if (string.IsNullOrEmpty(variableName))
		{
			return null;
		}
		BehaviorVariable lVariable = variables.Find(v => v.TrueVariableName == variableName);
		if (lVariable != null)
		{
			return lVariable;
		}
		else
		{
			if (ParentCodeScope == null)
			{
				{
					BehaviorVariable variable = behaviorEditor.ConfigurableVariables.Find(v => v.TrueVariableName == variableName);
					if (variable != null)
					{
						return variable;
					}
				}
				{
					BehaviorVariable variable = behaviorEditor.GlobalVariables.Find(v => v.TrueVariableName == variableName);
					if (variable != null)
					{
						return variable;
					}
				}
				{
					BehaviorVariable variable = behaviorEditor.AccessedSharedVariables.Find(v => v.TrueVariableName == variableName);
					if (variable != null)
					{
						return variable;
					}
				}
				return null;
			}
			return ParentCodeScope.FindVariableByName(variableName);
		}
	}
	
	public BehaviorVariable FindConflictingVariable(BehaviorVariable variable)
	{
		BehaviorVariable localVariable = variables.Find(v => v.TrueVariableName == variable.TrueVariableName && v != variable);
		if (localVariable != null)
		{
			return localVariable;
		}
		else
		{
			if (ParentCodeScope == null)
			{
				BehaviorVariable variable1 = behaviorEditor.ConfigurableVariables.Find(v => v.TrueVariableName == variable.TrueVariableName && v != variable);
				if (variable1 != null)
				{
					return variable1;
				}
				variable1 = behaviorEditor.GlobalVariables.Find(v => v.TrueVariableName == variable.TrueVariableName && v != variable);
				if (variable1 != null)
				{
					return variable1;
				}
				variable1 = behaviorEditor.AccessedSharedVariables.Find(v => v.TrueVariableName == variable.TrueVariableName && v != variable);
				if (variable1 != null)
				{
					return variable1;
				}
				return null; // TODO: Account for Shared Variables
			}
			return ParentCodeScope.FindConflictingVariable(variable);
		}
	}

	public List<OperationReference> ReadScope()
	{
		if (GetChildCount() == 0)
		{
			return null;
		}
		List<OperationReference> result = new();
		bool isTraceless = false;
		foreach (Node child in GetChildren())
		{
			Operation operation = (Operation)child;
			result.Add(operation.GetOperationReference());
			if (isTraceless)
			{
				isTraceless = operation.IsStateless;
			}
		}
		return result;
	}

	public bool IsStateless()
	{
		foreach (Node child in GetChildren())
		{
			Operation operation = (Operation)child;
			if (!operation.IsStateless)
			{
				return false;
			}
		}
		return true;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		return data.As<PackedScene>() != null;
	}

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		Operation operation = data.As<PackedScene>().Instantiate<Operation>();
		AddChild(operation);
		operation.Initialise(this, behaviorEditor); // TODO: instantiate the scene at the place where the cursor is, not at the end like it currently does
	}

	public void WriteScope(List<OperationReference> sequence)
	{
		foreach (OperationReference action in sequence)
		{
			Operation comp = behaviorEditor.GetOperationScene(action.OperationName).Instantiate<Operation>();
			AddChild(comp);
			comp.Initialise(this, behaviorEditor);
			comp.Write(action.Argumens);
		}
	}
}
