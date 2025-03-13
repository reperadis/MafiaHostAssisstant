using Godot;

namespace MafiaHostAssistant;

public partial class BEItemCard : Node
{
	[Export] private Label label;
	private bool isUsed;
	private int indexInUnused;
	private BEItemSelectionField selectionField;

	public void SetUp(string str, BEItemSelectionField selectionField)
	{
		label.Text = str;
		this.selectionField = selectionField;
		indexInUnused = GetIndex();
	}

	public void Move() // Button
	{
		if (isUsed)
		{
			selectionField.MoveItemToUnused(GetIndex(), indexInUnused);
			isUsed = false;
		}
		else
		{
			selectionField.MoveItemToUsed(GetIndex());
			isUsed = true;
		}
	}

	public string GetLabel()
	{
		return label.Text;
	}
}
