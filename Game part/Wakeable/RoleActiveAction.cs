using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public class RoleActiveAction : Wakeable
{
	private PersistentScope wakingAlgorythmScope;
	private PersistentScope actionScope;
	public string DisplayName { get; private set; }
	public Player Player { get; private set; } // The player whose role defines this action
	public bool IsTraceless { get; private set; }
	public string ActionFileName { get; private set; }

	private GameStateManager gameManager;
	private ConfiguredBehaviorRecord heldWARef;
	private ConfiguredBehaviorRecord heldActionRef;

	private WakeableHandlerWindow handler;

	public void Setup(Player player, ConfiguredBehaviorRecord waRef, ConfiguredBehaviorRecord actionRef, string displayName)
	{
		Player = player;
		DisplayName = displayName;
		ActionFileName = actionRef.behaviorName;
		heldActionRef = actionRef;
		heldWARef = waRef;
	}

	public void DiscardWA()
	{
		wakingAlgorythmScope = null;
	}

    public override async Task Initialize()
    {
		BehaviorRecord actionInfo = heldActionRef.ToBehaviorRecord(BehaviorType.ActiveActionWithPlayer);
		actionScope = new();
		await actionScope.Initialize(
			behaviorInfo: actionInfo,
			config: heldActionRef.config,
			behaviorName: heldActionRef.behaviorName,
            behaviorType: BehaviorType.ActiveActionWithPlayer,
			scopeOwner: Player,
            exitCancellationToken: Player.ExitCancellationToken
		);

		BehaviorRecord waInfo = heldWARef.ToBehaviorRecord(BehaviorType.WakingAlgorythm);
		wakingAlgorythmScope = new();
		await wakingAlgorythmScope.Initialize(
			behaviorInfo: waInfo,
			config: heldWARef.config,
			behaviorName: heldWARef.behaviorName,
            behaviorType: BehaviorType.WakingAlgorythm,
			scopeOwner: Player,
			exitCancellationToken: Player.ExitCancellationToken
		);

		IsTraceless = actionInfo.leavesNoTrace && waInfo.leavesNoTrace;
		heldActionRef = null;
		heldWARef = null;
	}

	public override async Task<bool> CanExecute(WakeableHandlerWindow handler)
	{
		// TODO: The player being dead not neccessary means that the action must not execute,
		// what if the players want to add some ghosty roles?
		return Player.IsAlive && Player.IsRoleActive && await wakingAlgorythmScope.TryExecute(null, handler);
	}

	public override void PrepareExecution(WakeableHandlerWindow handler)
    {
        this.handler = handler;
    }

    public override bool CanProceedToExecution()
    {
		return true;
    }

    public override async Task ProceedToExecution()
    {
		await actionScope.TryExecute(null, handler);
	}

    public override void CancelBeforeExecution()
    {
		// TODO: Maybe something should be done?
	}

}