using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public sealed partial class OP_CreateSharedVariable : OPVarCreator
{
	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		if (ParentScope.IdentationLevel != 0)
		{
			Delete();
			return;
		}
		base.OnAddition(behaviorEditor);
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.CreateSharedVariable, new Arguments(myVariable.TrueVariableName, myVariable.VariableType));
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;

        myVariable.TrueVariableName = args.varName;
		myVariable.VariableType = args.varType;

		varNameLabel.Text = myVariable.TranslatedVariableName;
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(args.varType);

		behaviorEditor.CreatedSharedVariables.Add(myVariable);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_CREATE-SHARED-VARIABLE");
	}

	public override bool IsStateless
	{
		get => false;
	}
	
	protected override (List<BehaviorVariable> operatedList, Action<BehaviorVariable> variableAddedAction, bool isVariablePersisting) GetOperatedItems()
	{
		return (behaviorEditor.CreatedSharedVariables, behaviorEditor.OnVariableAddedOrRenamed, true);
    }
}