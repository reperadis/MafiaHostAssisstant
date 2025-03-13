using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class NL_PlayerVsPlayers : NightLog
{
    [Export] private Label actionLabel;
    [Export] private Label acterLabel;
    [Export] private PackedScene hButtonScene;
    [Export] private Control targetsContent;
    private GameStateManager gameManager;
    private Player acter;

    public void SetUp(Player acter, string actionName, List<Player> targets, GameStateManager gameManager)
    {
        this.acter = acter;
        this.gameManager = gameManager;
        acterLabel.Text = acter.roleInfo.roleName;
        actionLabel.Text = $"({actionName})";

        foreach (Player player in targets)
        {
            HoldableButton button = hButtonScene.Instantiate<HoldableButton>();
            targetsContent.AddChild(button);
            button.Held += () => HighlightPlayer(player);
            button.Text = player.PlayerName;
        }
    }

    public void HighlightActer() // H-button which already exists
    {
        gameManager.UnobscurePlayersList();
        acter.HighlightPlayerCard();
    }
    
    private void HighlightPlayer(Player player)
    {
        gameManager.UnobscurePlayersList();
        player.HighlightPlayerCard();
    }

    /*public override void AddNote((string, bool)[] evaluatedNote)
    {
        throw new System.NotImplementedException();
    }

    public override NightLogSave GetLogSave()
    {
        return new Save();
    }

    public class Save : NightLogSave
    {

    }*/
}
