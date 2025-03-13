using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class TypeSearchField : Dropdown
{
	private Action<BehaviorVariable> receiver;

	private readonly List<ElementData> dropdownOptions = new();
	private readonly List<BehaviorVariable> foundVariables = new();

	// Setting current does not invoke Redirect;
	public virtual void SetUp(BehaviorVariable current, OperationScope rootCodeScope, BehaviorVariableType searchedVarType, bool excludeReadonlyVariables, BehaviorEditor behaviorEditor, Action<BehaviorVariable> receiver)
	{
		this.receiver = receiver;
		bool isSearchingForAnything = searchedVarType == BehaviorVariableType.Anything;
		bool isSearchingForLists = searchedVarType == BehaviorVariableType.ListOfAnything;
		OperationScope currentScope = rootCodeScope;

		if (searchedVarType != BehaviorVariableType.Anything) // To prevent duplicate search results for "@Null"
		{
			foundVariables.Add(BehaviorEditor.NullVariable);
			dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Nothing), BehaviorEditor.NullVariable.TranslatedVariableName));
		}

		while (currentScope != null)
		{
			foreach (BehaviorVariable variable in currentScope.variables)
			{
				if (excludeReadonlyVariables && variable.IsReadOnly)
				{
					continue;
				}
				if (isSearchingForAnything || variable.VariableType == searchedVarType)
				{
					foundVariables.Add(variable);
					dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
					continue;
				}
				if (isSearchingForLists && variable.IsList())
				{
					foundVariables.Add(variable);
					dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
				}
			}
			currentScope = currentScope.ParentCodeScope;
		}
		foreach (BehaviorVariable variable in behaviorEditor.GlobalVariables)
		{
			if (excludeReadonlyVariables && variable.IsReadOnly)
			{
				continue;
			}
			if (isSearchingForAnything || variable.VariableType == searchedVarType)
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
				continue;
			}
			if (isSearchingForLists && variable.IsList())
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
			}
		}
		foreach (BehaviorVariable variable in behaviorEditor.ConfigurableVariables)
		{
			if (excludeReadonlyVariables && variable.IsReadOnly)
			{
				continue;
			}
			if (isSearchingForAnything || variable.VariableType == searchedVarType)
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
				continue;
			}
			if (isSearchingForLists && variable.IsList())
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
			}
		}
		foreach (BehaviorVariable variable in behaviorEditor.AccessedSharedVariables)
		{
			if (isSearchingForAnything || variable.VariableType == searchedVarType)
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
			}
			if (isSearchingForLists && variable.IsList())
			{
				foundVariables.Add(variable);
				dropdownOptions.Add(new ElementData(Cache.Instance.GetVariableTypeTexture(variable.VariableType), variable.TranslatedVariableName));
			}
		}

		AddElements(dropdownOptions);
		
		if (current != null)
		{
			Current = foundVariables.IndexOf(current);
		}
		// TODO: is it needed this way? Decide on the general rule for every field: whether or not should the field invoke Redirect on its start
		ItemSelected += Redirect; // So as to not invoke the redirection yet
	}

	public void Redirect(int index)
	{
		receiver.Invoke(foundVariables[index]);
	}
}
