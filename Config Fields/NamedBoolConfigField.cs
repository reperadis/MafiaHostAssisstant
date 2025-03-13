using Godot;
using System;

namespace MafiaHostAssistant;

public partial class NamedBoolConfigField : NamedConfigField
{
	[Export] public Label label;
	[Export] private BoolConfigField field;

	public void SetUp(string label, bool current, Action<bool> receiver)
	{
		this.label.Text = label;
		field.SetUp(current, receiver);
	}
}
