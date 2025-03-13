using Godot;
using System;

namespace MafiaHostAssistant;
// TODO: This is a replacement to H-buttons that represent players, such as in Asking Notification; possibly will be used in Union View
public partial class SelectablePlayerCardClone : Control
{
    [Export] private Label playerNameLabel;
    [Export] private Label playerRoleLabel;
    [Export] private Control checkmark;
    private Action<Player> listener;
    private bool selected;
    private Player player;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            checkmark.Visible = value;
        }
    }

    public void SetUp(Player player, Action<Player> listener)
    {
        this.listener = listener;
        this.player = player;
        playerNameLabel.Text = player.Name;
        playerRoleLabel.Text = player.roleInfo.roleName;
    }

    public void ToggleAndRedirect()
    {
        Selected = !selected;
        listener.Invoke(player);
    }
}
