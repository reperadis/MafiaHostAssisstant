using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

// Autotranslation is toggled on
public partial class NamedEnumConfigField : NamedConfigField
{
	[Export] private Label label;
	[Export] private Dropdown dropdown;
	private Action<int> receiver;
	
	public void SetUp(string label, int current, IEnumerable<Dropdown.ElementData> options, Action<int> receiver)
	{
		this.label.Text = label;
		this.receiver = receiver;
		
		dropdown.AddElements(options);
		
		dropdown.Current = current;
	}
	
	public void Redirect(int selected)
	{
		receiver?.Invoke(selected);
	}
}
