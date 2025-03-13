using Godot;
using System;

namespace MafiaHostAssistant;

public partial class GameSceneManager : Node
{
	[Export] private PlayerNamingStateManager playerNamingStateManager;
	[Export] private RolePassingStateManager rolePassingStateManager;
	[Export] private GameStateManager gameStateManager;

	public void ContinueToRolePassingState() // Button in player naming state
	{
		playerNamingStateManager.Visible = false;
		rolePassingStateManager.Visible = true;
		rolePassingStateManager.SetUp(playerNamingStateManager.GetPlayerNames());
	}

	public void ContinueToGameState()
	{
		rolePassingStateManager.Visible = false;
		gameStateManager.Visible = true;
		gameStateManager.SetUp(rolePassingStateManager.GetNameRolePairs());
	}

    public override void _Notification(int what)
    {
		if (what == NotificationApplicationPaused && gameStateManager.Visible)
		{
			SaveGame();
		}
    }

	public void ExitPreGame()
	{
		GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
	}

	public void ExitGame()
	{
		SaveGame();
		GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
	}

	private void SaveGame()
	{
		// TODO: Save the players, notes and logs
	}

	[Serializable]
	private class GameSave
	{

	}
}
