using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

// Autotranslation is toggled on
// TODO: The scene has Arrow Icon texture rect. Assign an arrow down icon to make it obvious that this is a dropdown
public partial class Dropdown : Control
{
	[Export] private PackedScene elementScene;
	[Export] private Control elementsPanel;
	[Export] private Control elementsContent;
	[Export] private Label selectedElementLabel;
	[Export] private TextureRect selectedElementIcon;

	[Signal] public delegate void ItemSelectedEventHandler(int index);

	private int current = -1;
	public int Current // Zero-based indexing
	{
		get => current;
		set
		{
			current = value;
			DropdownElement element = elementsContent.GetChild<DropdownElement>(value);
			selectedElementIcon.Texture = element.Icon;
			selectedElementLabel.Text = element.Label;
			EmitSignal(SignalName.ItemSelected, value);
		}
	}

	public void AddElements(IEnumerable<ElementData> elements)
	{
		foreach (ElementData element in elements)
		{
			DropdownElement de = elementScene.Instantiate<DropdownElement>();
			de.Initialise(this, element.icon, element.label);
			elementsContent.AddChild(de);
		}
	}
	
	public void OnDropdownPresed()
	{
		elementsPanel.Visible = true;
	}
	
	public void SelectElement(int index)
	{
		current = index;
		DropdownElement element = elementsContent.GetChild<DropdownElement>(index);
		selectedElementIcon.Texture = element.Icon;
		selectedElementLabel.Text = element.Label;
		elementsPanel.Visible = false;
		EmitSignal(SignalName.ItemSelected, index);
	}
	
	public class ElementData
	{
		public Texture2D icon;
		public string label;

		public ElementData(Texture2D icon, string label)
		{
			this.icon = icon;
			this.label = label;
		}
	}
}
