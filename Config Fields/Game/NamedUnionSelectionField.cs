using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class NamedUnionSelectionField : Control
{
	[Export] public Label label;
    [Export] private Control content;
    [Export] private PackedScene selectionButtonScene;
    private SelectionHButton last;
    private Action<ActionUnion> receiver;

    public void SetUp(string label, ActionUnion current, GameStateManager manager, Action<ActionUnion> receiver)
    {
        this.label.Text = label;
        this.receiver = receiver;

        CreateButtonsRecursive(manager.RootWakeables, current, string.Empty);
    }

    private void CreateButtonsRecursive(List<Wakeable> wakeables, ActionUnion selected, string prefix) // TODO: Not urgent. Button text may overflow if the chain is too big
    {
        foreach (Wakeable wakeable in wakeables)
        {
            if (wakeable is ActionUnion union)
            {
                SelectionHButton button = selectionButtonScene.Instantiate<SelectionHButton>();
                button.CustomMinimumSize = new Vector2(130, 630);
                button.hButton.Text = prefix + union.UnionName;
                content.AddChild(button);
                button.hButton.Held += () => ToggleAndRedirect(button, union);
                if (union == selected)
                {
                    button.Selected = true;
                }
            }
            if (wakeable is WakeableCombination combination)
            {
                CreateButtonsRecursive(combination.ContainedWakeables, selected,  prefix + $"({combination.CombinationName}) -> ");
            }
        }
    }

    private void ToggleAndRedirect(SelectionHButton btn, ActionUnion player)
    {
        if (last != null)
        {
            last.Selected = false;
        }
        last = btn;
        receiver?.Invoke(player);
    }
}
