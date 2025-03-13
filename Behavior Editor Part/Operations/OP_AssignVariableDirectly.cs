using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_AssignVariableDirectly : Operation
{
	[Export] private Label variableNameLabel;
	[Export] private TextureRect variabletypeTextureRect;

	private static readonly string[] incomatibleAssignedTypeErrorPath = { "" }; // TODO: path is empty
	private BehaviorEditor behaviorEditor;

	private object assignedValue;

	private BehaviorVariableHandler variableHandler;
	private int incomaptibleAssignedTypeErrorIndex = -1;
	private Control variableSetterField;
	private BehaviorVariableType previousType = BehaviorVariableType.Nothing;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		variableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Anything, variableNameLabel, variabletypeTextureRect);
		variableHandler.PostVariableRegistered += OnPostRegisterVariable;
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		variableHandler.CreateSelectionField(true);
		if (variableHandler.Variable != null)
		{
			TryCreateValueField();
		}
	}

	public void OnPostRegisterVariable()
	{
		if (variableHandler.Variable.VariableType == previousType)
		{
			return;
		}

		if (variableSetterField != null)
		{
			variableSetterField.QueueFree();
			variableSetterField = null;
		}
	}

	private void TryCreateValueField()
	{
		switch (variableHandler.Variable.VariableType)
		{
			case BehaviorVariableType.Bool:
				variableSetterField = behaviorEditor.CreateBoolField(Tr("TK:VALUE"), false, CatchBool);
				void CatchBool(bool value)
				{
					assignedValue = value;
				}
				break;
			case BehaviorVariableType.Integer:
				variableSetterField = behaviorEditor.CreateIntField(Tr("TK:VALUE"), 0, true, CatchInt);
				void CatchInt(int value)
				{
					assignedValue = value;
				}
				break;
			case BehaviorVariableType.String:
				variableSetterField = behaviorEditor.CreateStringField(Tr("TK:VALUE"), string.Empty, CatchString);
				void CatchString(string value)
				{
					assignedValue = value;
				}
				break;
			case BehaviorVariableType.ListOfBools:
				List<BEListElementData> tempoListBools = new();
				assignedValue = tempoListBools;
				variableSetterField = behaviorEditor.CreateBEListField(
					Tr("TK:VALUE"), ParentScope, BehaviorVariableType.Bool, tempoListBools);
				break;
			case BehaviorVariableType.ListOfInts:
				List<BEListElementData> tempoListInts = new();
				assignedValue = tempoListInts;
				variableSetterField = behaviorEditor.CreateBEListField(
					Tr("TK:VALUE"), ParentScope, BehaviorVariableType.Integer, tempoListInts);
				break;
			case BehaviorVariableType.ListOfStrings:
				List<BEListElementData> tempoListStrings = new();
				assignedValue = tempoListStrings;
				variableSetterField = behaviorEditor.CreateBEListField(
					Tr("TK:VALUE"), ParentScope, BehaviorVariableType.String, tempoListStrings);
				break;
			case BehaviorVariableType.ListOfPlayers:
				if (incomaptibleAssignedTypeErrorIndex == -1)
				{
					ResolveError(incomaptibleAssignedTypeErrorIndex);
					incomaptibleAssignedTypeErrorIndex = -1;
				}
				List<BEListElementData> tempoListPlayers = new();
				assignedValue = tempoListPlayers;
				variableSetterField = behaviorEditor.CreateBEListField(Tr("TK:VALUE"), ParentScope, BehaviorVariableType.Player, tempoListPlayers);
				break;

			case BehaviorVariableType.Player:
				goto default;
			case BehaviorVariableType.Union:
				goto default;
			default: // If variable cannot be assigned directly
				if (incomaptibleAssignedTypeErrorIndex == -1)
				{
					incomaptibleAssignedTypeErrorIndex = PushError(incomatibleAssignedTypeErrorPath, ConstructIncompatibleTypeError(variableHandler.Variable.VariableType), false);
				}
				break;
		}
	}

    protected override void OnDeletion()
    {
		variableHandler.PostVariableRegistered -= OnPostRegisterVariable;
    }

    public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.AssignDirectly, new Arguments(variableHandler.Variable.TrueVariableName, assignedValue));
	}
	
	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		variableHandler.RegisterVariable(ParentScope.FindVariableByName(args.assingToVarName));
		assignedValue = args.assignedValue;
	}
	
	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_ASSIGN-DIRECTLY");
	}

    public override bool IsStateless => true;

    [Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public string assingToVarName;
		public object assignedValue;

		public Arguments(string assingToVarName, object assignedValue)
		{
			this.assingToVarName = assingToVarName;
			this.assignedValue = assignedValue;
		}
	}

	private static string ConstructIncompatibleTypeError(BehaviorVariableType variableType)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Cannot directly assign a value to a variable of type {variableType.ToTranslatedFormatedStringLowercase()}!";
		}
		else
		{
			return $"Невозможно вручную присвоить значение переменной типа {variableType.ToTranslatedFormatedStringLowercase()}!";
		}
	}
}
