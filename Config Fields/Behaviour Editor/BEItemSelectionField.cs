using System.Collections.Generic;
using System.Linq;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class BEItemSelectionField : Control
{
	[Export] private VBoxContainer unusedItemsContent;
	[Export] private PackedScene itemScene;
	[Export] private VBoxContainer usedItemsContent;
	[Export] private BehaviorEditor behaviorEditor;
	[Export] private StringConfigField newItemStringField;
	
	private string newItemString;

	public void SetUp(IEnumerable<string> items)
	{
		foreach (string str in items)
		{
			BEItemCard card = itemScene.Instantiate<BEItemCard>();
			unusedItemsContent.AddChild(card);
			card.SetUp(str, this);
		}
		newItemStringField.SetUp(string.Empty, StringFieldContext.Unrestricted, false, s => newItemString = s);
	}

	public void MoveItemToUsed(int indexInUnused)
	{
		BEItemCard card = unusedItemsContent.GetChild<BEItemCard>(indexInUnused);
		unusedItemsContent.RemoveChild(card);
		usedItemsContent.AddChild(card);
		if (usedItemsContent.GetChildCount() == 1)
		{
			usedItemsContent.Visible = true;
		}
		if (unusedItemsContent.GetChildCount() == 0)
		{
			unusedItemsContent.Visible = false;
		}
	}

	public void MoveItemToUnused(int indexInUsed, int indexInUnused)
	{
		BEItemCard card = usedItemsContent.GetChild<BEItemCard>(indexInUsed);
		usedItemsContent.RemoveChild(card);
		unusedItemsContent.AddChild(card);

		if (unusedItemsContent.GetChildCount() <= indexInUnused)
		{
			unusedItemsContent.MoveChild(card, -1);
		}
		else
		{
			unusedItemsContent.MoveChild(card, indexInUnused);
		}

		if (usedItemsContent.GetChildCount() == 0)
		{
			usedItemsContent.Visible = false;
		}
	}

	public void AddNewItem() // Button
	{
		BEItemCard card = itemScene.Instantiate<BEItemCard>();
		usedItemsContent.AddChild(card);
		card.SetUp(newItemString, this);
	}

	public string[] GetItems()
	{
		return usedItemsContent.GetChildren().Select(i => (i as BEItemCard).GetLabel()).ToArray();
	}

	public void SetUsedItems(List<string> items)
	{
		foreach	(string item in items)
		{
			BEItemCard card = unusedItemsContent.GetChildren().Cast<BEItemCard>().FirstOrDefault(i => i.GetLabel() == item);
			if (card == null)
			{
				card = itemScene.Instantiate<BEItemCard>();
				card.SetUp(item, this);
			}
			usedItemsContent.AddChild(card);
		}
	}
}
