using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MafiaHostAssistant;

public abstract partial class Operation : Control
{
	[Export] private Theme darkTheme;
	[Export] private Theme lightTheme;
	[Export] private Control identationControl;
	[Export] private Button deselectionButton;
	public OperationScope ParentScope {get; private set; }
	
	public void Initialise(OperationScope scope, BehaviorEditor behaviorEditor)
	{
		ParentScope = scope;
		// TODO: Make identationControl width be configurable in settings

		if (this is OPVarCreator and not OP_CreateSharedVariable)
		{
			int lowestAllowedIndex = 0;
			foreach (Node node in ParentScope.GetChildren())
			{
				if (node is not OP_CreateSharedVariable)
				{
					break;
				}
				lowestAllowedIndex++;
			}
            ParentScope.CallDeferred(Node.MethodName.MoveChild, this, lowestAllowedIndex);
		}
		if (this is OP_CreateSharedVariable)
		{
			ParentScope.CallDeferred(Node.MethodName.MoveChild, this, 0);
		}

		Theme = Settings.Instance.IsLightTheme.Value ? lightTheme : darkTheme;
		Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);

		OnAddition(behaviorEditor);
	}
	
	public abstract OperationReference GetOperationReference();
	public abstract void Write(OperationReference.Arguments argumens);
	public abstract string GetReadableOpearationName();
	public abstract bool IsStateless { get; }
	protected virtual bool CanGetDeleted() { return true; }
	protected virtual void OnAddition(BehaviorEditor behaviorEditor) { }
	protected virtual void OnDeletion() { }
	
	public void MoveUp()
	{
		// Check the operation above us, if any
		if (GetIndex() == 0)
		{
			return; // If we are the first
		}

		Operation operationAbove = (Operation)ParentScope.GetChild(GetIndex() - 1);
		if (operationAbove is OP_CreateSharedVariable && this is not OP_CreateSharedVariable) // OP_CreateShared must be on the very top
		{
			return;
		}
		if (this is not OPVarCreator)
		{
			if (operationAbove is OPVarCreator)
			{
				return; // If only the variable creators are above and this is not one of them
			}
		}
		ParentScope.MoveChild(this, GetIndex() - 1);
	}
	
	public void MoveDown()
	{
		if (GetIndex() == ParentScope.GetChildCount() - 1)
		{
			return; // If we are the last operation
		}

		Operation operationBelow = (Operation)ParentScope.GetChild(GetIndex() + 1);
		if (operationBelow is not OP_CreateSharedVariable && this is OP_CreateSharedVariable)
		{
			return;
		}
		if (this is OPVarCreator)
		{
			if (operationBelow is not OPVarCreator)
			{
				return; // If we are the last variable creator
			}
		}
		ParentScope.MoveChild(this, GetIndex() + 1);
	}

	public void Select() // HButton
	{
		ParentScope.RootEntryPoint.SelectOperation(this);
		deselectionButton.Visible = true;
	}

	public void Deselect()
	{
		ParentScope.RootEntryPoint.DeselectOperation(this);
		deselectionButton.Visible = false;
	}

	public void SetDeselected()
	{
		deselectionButton.Visible = false;
	}
	
	public void Delete()
	{
		OnDeletion();
		ResolveAllErrorsIfAny();
		QueueFree();
	}

    private void OnThemeChanged(bool newIsLightTheme)
	{
		Theme = newIsLightTheme ? lightTheme : darkTheme;
	}

	public int PushError(string[] docsPath, string message, bool isSaveBlocking) => ParentScope.RootEntryPoint.PushError(this, docsPath, message, isSaveBlocking);
	public void ResolveError(int errorIndex) => ParentScope.RootEntryPoint.ResolveError(this, errorIndex);
	public void ResolveAllErrorsIfAny() => ParentScope.RootEntryPoint.ResolveAllErrorsIfAny(this);
	
	public static List<SharedDynamicStringElementData> BEToShared(List<BEDynamicStringElementData> elements)
	{
		return new(elements.Select(e =>
        {
            return new SharedDynamicStringElementData(
			isVariable: e.isVariable,
            stringData: e.isVariable ? e.variable.TranslatedVariableName : e.directString,
            variableType: e.isVariable ? e.variable.VariableType : BehaviorVariableType.Nothing);
        }));
	}
	
	public List<BEDynamicStringElementData> SharedToBE(List<SharedDynamicStringElementData> elements)
	{
		List<BEDynamicStringElementData> data = new();
		foreach (SharedDynamicStringElementData e in elements)
		{
            data.Add(new(e.isVariable ? ParentScope.FindVariableByName(e.stringData) : null, e.isVariable ? null : e.stringData, e.isVariable));
		}
		return data;
	}

	public List<DisplayableDynamicStringElementData> BEToDisplayable(List<BEDynamicStringElementData> be)
	{
		List<DisplayableDynamicStringElementData> displayable = new();
		foreach (BEDynamicStringElementData element in be)
		{
			IStringSource source = null;

			if (element.isVariable)
			{
				source = ParentScope.FindVariableByName(element.variable.TrueVariableName);
			}
			else
			{
				source = new DirectStringSource(element.directString);
			}

			displayable.Add(new DisplayableDynamicStringElementData(
				isVariable: element.isVariable,
				variableType: element.isVariable ? element.variable.VariableType : BehaviorVariableType.Nothing,
				stringSource: source
			));
		}
		return displayable;
	}

	public static string ConstructFieldIsEmptyError(string fieldNameKey)
	{
		string fieldName = TranslationServer.Translate(fieldNameKey);
		if (TranslationServer.GetLocale() == "en")
		{
			return $"The field \"{fieldName}\" cannot be empty!";
		}
		else // "ru"
		{
			return $"Поле \"{fieldName}\" не может быть пустым!";
		}
	}
	
	public static string ConstructVariableDeletedError(string variableName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Variable \"{variableName}\" no longer exists!";
		}
		else
		{
			return $"Переменная \"{variableName}\" больше не существует!";
		}
	}
	
	public static string ConstructOperationIncompatibleWithNewTypeError(string variableName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Operation is not compatible with the new type of variable \"{variableName}\"!";
		}
		else
		{
			return $"Операция несовместима с новым типом переменной \"{variableName}\"!";
		}
	}
}