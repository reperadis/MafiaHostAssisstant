using System;
using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class OP_CreateVariable : OPVarCreator
{
	private static readonly string[] VariableNameConflictsWithHigherPriorityErrorPath = { "Create Variable", "Errors" };
	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		base.OnAddition(behaviorEditor);
		ParentScope.OnVariableAddedOrRenamed += OnVariableAdded;
	}

	private void OnVariableAdded(BehaviorVariable variable)
	{
		if (variable != myVariable && myVariable.TrueVariableName == variable.TrueVariableName)
		{
			ResolveAllErrorsIfAny();
			PushError(VariableNameConflictsWithHigherPriorityErrorPath, ConstructVariableNameConflictsWithHigherPriorityError(myVariable.TranslatedVariableName), true);
			hasBadNameError = true;
		}
	}

    protected override void OnDeletion()
    {
		ParentScope.OnVariableAddedOrRenamed -= OnVariableAdded;
		base.OnDeletion();
    }

    public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.CreateVariable, new Arguments(myVariable.TrueVariableName, myVariable.VariableType));
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;


        myVariable.TrueVariableName = args.varName;
		myVariable.VariableType = args.varType;

		varNameLabel.Text = myVariable.TranslatedVariableName;
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(args.varType);

		ParentScope.variables.Add(myVariable);
		ParentScope.OnVariableAddedOrRenamed?.Invoke(myVariable);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_CREATE-VARIABLE");
	}

    public override bool IsStateless => true;

    protected override (List<BehaviorVariable> operatedList, Action<BehaviorVariable> variableAddedAction, bool isVariablePersisting) GetOperatedItems()
	{
		return (ParentScope.variables, ParentScope.OnVariableAddedOrRenamed, false);
	}
	
	private static string ConstructVariableNameConflictsWithHigherPriorityError(string variableName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Variable name {variableName} conflicts with the name of a variable of a higher priority!";
		}
		else
		{
			return $"Имя переменной {variableName} конфликтует с именем переменной более высокого приоритета!";
		}
	}
}
