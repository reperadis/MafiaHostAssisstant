using Godot;

namespace MafiaHostAssistant;

public partial class SelectionHButton : Control
{
	[Export] private TextureRect checkmark;
	[Export] public HoldableButton hButton;

	private bool selected;
	public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            checkmark.Visible = value;
        }
    }
}
