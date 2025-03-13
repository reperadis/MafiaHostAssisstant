using Godot;

namespace MafiaHostAssistant;

public partial class DropdownElement : Control
{
	[Export] private Label label;
	[Export] private TextureRect iconRect;

	private Dropdown dropdown;
	
	public Texture2D Icon { get; private set; }
	public string Label { get; private set; }
	
	public void Initialise(Dropdown dropdown, Texture2D icon, string label)
	{
		this.dropdown = dropdown;
		this.label.Text = label;
		if (icon == null)
		{
			iconRect.Visible = false;
		}
		else
		{
			iconRect.Texture = icon;			
		}
		Icon = icon;
		Label = label;
	}

	public void OnElementPressed()
	{
		dropdown.SelectElement(GetIndex());
	}
}
