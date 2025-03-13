using Godot;
using System;

namespace MafiaHostAssistant;

public partial class NamedTypeSearchField : NamedConfigField
{
	[Export] private Label label;
	[Export] private TypeSearchField field;

	public void SetUp(string label, BehaviorVariable current, OperationScope rootCodeScope, BehaviorVariableType searchedVarType, bool excludeReadonlyVariables, BehaviorEditor behaviorEditor, Action<BehaviorVariable> receiver)
	{
		this.label.Text = label;
		field.SetUp(current, rootCodeScope, searchedVarType, excludeReadonlyVariables, behaviorEditor, receiver);
	}
}
