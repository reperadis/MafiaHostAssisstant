using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class NamedPlayerSelectionField : Control
{
	[Export] public Label label;
    [Export] private Control content;
    [Export] private PackedScene selectionPlayerCardScene;
    private List<Player> list;
    private int acceptedCount;
    private Action listener;

    public void SetUp(string label, int acceptedCount, List<Player> current, GameStateManager manager, Action listener)
    {
        this.label.Text = label;
        this.acceptedCount = acceptedCount;
        list = current;
        this.listener = listener;

        foreach (Player player in manager.Players)
        {
            SelectablePlayerCardClone card = selectionPlayerCardScene.Instantiate<SelectablePlayerCardClone>();
            card.SetUp(player, ToggleAndRedirect);
            content.AddChild(card);
            if (current.Contains(player))
            {
                card.Selected = true;
            }
        }
    }

    private void ToggleAndRedirect(Player player)
    {
        if (list.Contains(player))
        {
            list.Remove(player);
        }
        else if (list.Count < acceptedCount)
        {
            list.Add(player);
        }
        listener?.Invoke();
    }
}