using Godot;

namespace MafiaHostAssistant;

public partial class PlayerNamingCard : Node
{
	[Export] public LineEdit nameLineEdit;
	private PlayerNamingStateManager manager;

	public void SetUp(PlayerNamingStateManager manager)
	{
		this.manager = manager;
	}

	public void TrySwitchToNextCard()
	{
		manager.TryFocusOnNextNameCard(GetIndex());
	}

	public void Remove()
	{
		QueueFree();
	}
}

