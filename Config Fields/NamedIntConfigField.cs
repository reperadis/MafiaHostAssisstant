using Godot;
using System;

namespace MafiaHostAssistant;

public sealed partial class NamedIntConfigField : NamedConfigField
{
	[Export] public Label label;
	[Export] private IntConfigField field;

	public void SetUp(string label, int current, bool allowNegative, Action<int> reciever)
	{
		this.label.Text = label;
		field.SetUp(current, allowNegative, reciever);
	}
}
