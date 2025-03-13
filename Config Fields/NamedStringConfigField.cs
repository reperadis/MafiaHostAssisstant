using Godot;
using System;

namespace MafiaHostAssistant;

public partial class NamedStringConfigField : NamedConfigField
{
	[Export] public Label label;
	[Export] private StringConfigField field;

	public void SetUp(string label, string current, StringFieldContext context, bool allowMultiline, Action<string> receiver)
	{
		this.label.Text = label;
		field.SetUp(current, context, allowMultiline, receiver);
	}
}
