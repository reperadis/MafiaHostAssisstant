using Godot;

namespace MafiaHostAssistant;

public sealed partial class NL_PlayerNoVictim : NightLog
{
    [Export] private Label actionLabel;
    [Export] private Label acterLabel;
    private GameStateManager gameManager;
    private Player acter;
    private bool isShowingPlayerName;

    public void SetUp(Player playerActer, string actionName, GameStateManager gameManager)
    {
        this.gameManager = gameManager;
        acter = playerActer;
        actionLabel.Text = actionName;
        acterLabel.Text = playerActer.roleInfo.roleName;
    }

    public void ToggleActerInfo() // Button
    {
        if (isShowingPlayerName)
        {
            isShowingPlayerName = false;
            acterLabel.Text = acter.roleInfo.roleName;
        }
        else
        {
            isShowingPlayerName = true;
            acterLabel.Text = acter.PlayerName;
        }
    }

    public void HighlightActer() // Acter HButton
    {
        gameManager.UnobscurePlayersList();
        acter.HighlightPlayerCard();
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
