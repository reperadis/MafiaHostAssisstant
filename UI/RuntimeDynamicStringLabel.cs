namespace MafiaHostAssistant;

public partial class RuntimeDynamicStringLabel : DynamicStringDisplayer
{
	private GameStateManager gameManager;
	
	public void SetUp(GameStateManager gameManager)
	{
		this.gameManager = gameManager;
	}

	protected override void OnPlayerButtonHeld(string playerName)
	{
		Player player = gameManager.Players.Find(player => player.PlayerName == playerName);
		gameManager.UnobscurePlayersList();
		player.HighlightPlayerCard();
	}
}