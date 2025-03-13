using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public sealed partial class OP_CreateConfig : OPVarCreator
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
        return new OperationReference(OperationName.CreateConfig, new Arguments(myVariable.TrueVariableName, myVariable.VariableType));
    }

    public override void Write(OperationReference.Arguments argumens)
    {
        Arguments args = (Arguments)argumens;

        myVariable.TrueVariableName = args.varName;
        myVariable.VariableType = args.varType;

        varNameLabel.Text= myVariable.TranslatedVariableName;
        varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(args.varType);

        behaviorEditor.ConfigurableVariables.Add(myVariable);
        behaviorEditor.OnVariableAddedOrRenamed?.Invoke(myVariable);
    }

    public override string GetReadableOpearationName()
    {
        return Tr("TK:OP_CREATE-CONFIG");
    }

    public override bool IsStateless
    {
        get => false;
    }

    protected override (List<BehaviorVariable> operatedList, Action<BehaviorVariable> variableAddedAction, bool isVariablePersisting) GetOperatedItems()
    {
        return (behaviorEditor.ConfigurableVariables, behaviorEditor.OnVariableAddedOrRenamed, true);
    }
}