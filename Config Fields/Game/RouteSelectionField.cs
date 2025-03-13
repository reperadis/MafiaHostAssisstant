using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class RouteSelectionField : Control
{
    [Export] private Label label;
    [Export] private Control buttonsContent;
    [Export] private PackedScene buttonScene;
    private Action<int> receiver;
    private SelectionHButton current;

    public void SetUp(string label, List<string> options, Action<int> receiver)
    {
        this.label.Text = label;
        this.receiver = receiver;

        for (int i = 0; i < options.Count; i++)
        {
            string option = options[i];

            SelectionHButton button = buttonScene.Instantiate<SelectionHButton>();
            button.hButton.Text = option;
            int frozenI = i;
            button.hButton.Held += () => ToggleAndRedirect(button, frozenI);
            buttonsContent.AddChild(button);
        }
    }

    private void ToggleAndRedirect(SelectionHButton btn, int index)
    {
        if (current != null)
        {
            current.Selected = false;
        }
        current = btn;
        receiver?.Invoke(index);
    }
}
