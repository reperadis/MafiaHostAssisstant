using System;
using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public abstract partial class OPVarCreator : Operation
{
	protected static List<Dropdown.ElementData> VarCreatorsTypeOptions { get; private set; }
	private static readonly string[] VariableMustHaveNameErrorPath = { "Variable Creators", "Errors", "Variable Must Have Name" }; // TODO: Consider referencing only the erros tab
	private static readonly string[] VariableNameAlreadyExistsErrorPath = { "Variable Creators", "Errors", "Variable Name Already Exists" };
	private static bool isOptionsInitialised = false;
	
	[Export] protected Label varNameLabel;
	[Export] protected TextureRect varTypeTextureRect;
	protected BehaviorEditor behaviorEditor;
	protected readonly BehaviorVariable myVariable = new(string.Empty, BehaviorVariableType.Bool, false);
	protected bool hasBadNameError = true;

	private List<BehaviorVariable> operatedVariableList;
	private Action<BehaviorVariable> operatedAddedOrRenamedAction;
    private BehaviorVariable currentConflict;

    protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		if (!isOptionsInitialised)
		{
			VarCreatorsTypeOptions = new()
			{
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Bool), Tr("TK:VARTYPE_BOOL")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Integer), Tr("TK:VARTYPE_INT")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.String), Tr("TK:VARTYPE_STRING")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Player), Tr("TK:VARTYPE_PLAYER")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Union), Tr("TK:VARTYPE_UNION")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.ListOfBools), Tr("TK:VARTYPE_LIST-BOOL")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.ListOfInts), Tr("TK:VARTYPE_LIST-INT")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.ListOfStrings), Tr("TK:VARTYPE_LIST-STRING")),
				new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.ListOfPlayers), Tr("TK:VARTYPE_LIST-PLAYER"))
			};
			isOptionsInitialised = true;
		}

		this.behaviorEditor = behaviorEditor;
		(operatedVariableList, operatedAddedOrRenamedAction, bool isVariablePersisting) = GetOperatedItems();
		myVariable.isPersisting = isVariablePersisting;

		myVariable.VariableType = BehaviorVariableType.Bool; // Default
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Bool);

		PushError(VariableMustHaveNameErrorPath, ConstructVariableMustHaveNameError(), false);
	}

	protected override void OnDeletion()
	{
		if (currentConflict != null)
		{
			currentConflict.OnVariableRemoved -= HandleConflictEvent;
			currentConflict.OnVariableRenamed -= HandleConflictEvent;
			currentConflict = null;
		}

		operatedVariableList.Remove(myVariable);
		myVariable.IsInvalid = true;
		myVariable.InvokeRemoval();
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		behaviorEditor.CreateStringField(Tr("TK:OP_FIELD_VARNAME"), myVariable.TrueVariableName, ReceiveVarName);
		behaviorEditor.CreateEnumField(Tr("TK:OP_FIELD_VARTYPE"), VarCreatorsTypeOptions, (int)myVariable.VariableType, ReceiveVarType);
	}

	private void ReceiveVarName(string value)
	{
		if (currentConflict != null)
		{
			currentConflict.OnVariableRemoved -= HandleConflictEvent;
			currentConflict.OnVariableRenamed -= HandleConflictEvent;
			currentConflict = null;
		}


		myVariable.TrueVariableName = value;
		varNameLabel.Text = value;

		if (string.IsNullOrEmpty(value))
		{
			if (hasBadNameError)
			{
				ResolveAllErrorsIfAny();
			}
			varNameLabel.Text = "@Null";
			PushError(VariableMustHaveNameErrorPath, ConstructVariableMustHaveNameError(), false);
			hasBadNameError = true;
			return;
		}

		BehaviorVariable conflict = ParentScope.FindConflictingVariable(myVariable);
		if (conflict != null)
		{
			conflict.OnVariableRemoved += HandleConflictEvent;
			conflict.OnVariableRenamed += HandleConflictEvent;
			currentConflict = conflict;

			ResolveAllErrorsIfAny();
			PushError(VariableNameAlreadyExistsErrorPath, ConstructVariableNameAlreadyExistsError(value), true);
			hasBadNameError = true;
			return;
		}

		if (!hasBadNameError && !string.IsNullOrEmpty(myVariable.TrueVariableName)) // We rename it
		{
			operatedAddedOrRenamedAction?.Invoke(myVariable);
			return;
		}

		if (hasBadNameError)
		{
			ResolveAllErrorsIfAny();
			hasBadNameError = false;
		}

		varNameLabel.Text = value;
		// We allocate new; it being not present is checked already
		operatedVariableList.Add(myVariable);
		myVariable.IsInvalid = false;
		operatedAddedOrRenamedAction?.Invoke(myVariable);
	}

	private void ReceiveVarType(int value)
	{
		if ((int)myVariable.VariableType == value)
		{
			return;
		}
		BehaviorVariableType type = (BehaviorVariableType)value;
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(type);
		// TODO: Check if this is OP_CreateConfig and push an error for incompatible type if the selected type is Player, Union or ListPlayer
		myVariable.VariableType = type;
		if (!string.IsNullOrEmpty(myVariable.TrueVariableName) && operatedVariableList.Contains(myVariable)) // We change type
		{
			if (type == BehaviorVariableType.Nothing) // Changing to nothing means deleting
			{
				operatedVariableList.Remove(myVariable);
				myVariable.IsInvalid = true;
				myVariable.InvokeRemoval();
				return;
			}
			return;
		}
		if (!string.IsNullOrEmpty(myVariable.TrueVariableName)) // We allocate new
		{
			operatedVariableList.Add(myVariable);
			myVariable.IsInvalid = false;
			operatedAddedOrRenamedAction?.Invoke(myVariable);
		}
	}

	private void HandleConflictEvent()
	{
		// Unsubscribe from events
		if (currentConflict != null)
		{
			currentConflict.OnVariableRemoved -= HandleConflictEvent;
			currentConflict.OnVariableRenamed -= HandleConflictEvent;
			currentConflict = null;
		}

		// To re-validate
		ReceiveVarName(myVariable.TrueVariableName);
	}

	[Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public string varName;
		public BehaviorVariableType varType;

		public Arguments(string varName, BehaviorVariableType varType)
		{
			this.varName = varName;
			this.varType = varType;
		}
	}

	protected static string ConstructVariableMustHaveNameError()
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return "Variable must have a name!";
		}
		else
		{
			return "Переменная должна иметь имя!";
		}
	}
	
	protected static string ConstructVariableNameAlreadyExistsError(string variableName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Variable with the name \"{variableName}\" already exists!";
		}
		else
		{
			return $"Переменная с именем \"{variableName}\" уже существует!";
		}
	}
	
	protected abstract (List<BehaviorVariable> operatedList, Action<BehaviorVariable> variableAddedAction, bool isVariablePersisting) GetOperatedItems();
}
