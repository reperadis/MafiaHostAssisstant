using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public sealed partial class Player : Control, IScopeOwner, IStringSource
{
	[Export] private Label playerNameLabel; // On the ActivityButton
	[Export] private Label playerRoleLabel;
	[Export] private Control orderDragHandle;

	private GameStateManager gameManager;

	private Color originalColor; // For Highlighting
	private int lateSiblingIndex;

	public RoleRecord roleInfo;
	public string PlayerName;
	public bool IsAlive { get; private set; } = true;
	public bool IsRoleActive { get; private set; } = true;
	public CancellationToken ExitCancellationToken {
		get => exitCancellationTokenSource.Token;
	}
	public List<RoleActiveAction> RoleActions { get; } = new();

	private PersistentScope passivePlayerActionScope;
	private PersistentScope passiveUnionActionScope;
	public readonly CancellationTokenSource exitCancellationTokenSource = new();

	public event Action PlayerNameChanged; // TODO: Remember this exists

    public void SetUp(GameStateManager gameManager, string playerName, RoleRecord roleInfo)
	{
		lateSiblingIndex = GetIndex();
		this.gameManager = gameManager;
		/*playerCardsSorter = FindAnyObjectByType<PlayerCardsSorter>();*/
		
		/*foreach ((ConfiguredBehaviorRecord actionRef, ConfiguredBehaviorRecord wakingAlgorythmRef, string displayName) in roleInfo.activeActions)
		{
			RoleAction activeAction = new();
			activeAction.Setup(this, wakingAlgorythmRef, actionRef, displayName);
			RoleActions.Add(activeAction);
		}*/

		this.PlayerName = playerName;
		this.roleInfo = roleInfo;
		playerNameLabel.Text = playerName;
		playerRoleLabel.Text = roleInfo.roleName;
	}

	public async Task Initialize()
	{
		if (roleInfo.passivePlayerActionRef != null)
		{
			passivePlayerActionScope = new();
			BehaviorRecord passivePlayerActionInfo = roleInfo.passivePlayerActionRef.ToBehaviorRecord(BehaviorType.PassiveActionWithUnion);
			await passivePlayerActionScope.Initialize(
                behaviorInfo: passivePlayerActionInfo,
				config: roleInfo.passivePlayerActionRef.config,
				behaviorName: roleInfo.passivePlayerActionRef.behaviorName,
                behaviorType: BehaviorType.PassiveActionWithPlayer,
                scopeOwner: this,
                exitCancellationToken: ExitCancellationToken
			);
		}
		if (roleInfo.passiveUnionActionRef != null)
		{
			passiveUnionActionScope = new();
			BehaviorRecord passiveUnionActionInfo = roleInfo.passiveUnionActionRef.ToBehaviorRecord(BehaviorType.PassiveActionWithUnion);
			await passiveUnionActionScope.Initialize(
                behaviorInfo: passiveUnionActionInfo,
				config: roleInfo.passiveUnionActionRef.config,
                behaviorName: roleInfo.passiveUnionActionRef.behaviorName,
                behaviorType: BehaviorType.PassiveActionWithUnion,
                scopeOwner: this,
                exitCancellationToken: ExitCancellationToken
			);
		}
		// Initialisation of RoleActions is performed by the containing Wakeable or GameManager
	}

	public async Task InvokePassivePlayerAction(Player player, WakeableHandlerWindow handler)
	{
		if (!IsRoleActive)
		{
			return;
		}
		await passivePlayerActionScope.TryExecute(new Dictionary<string, VariableContainer> { { "@TGT", new VariableContainer<Player>(player) } }, handler);
	}

	public async Task InvokePassiveUnionAction(ActionUnion roleUnion, WakeableHandlerWindow handler)
	{
		if (!IsRoleActive)
		{
			return;
		}
		await passiveUnionActionScope.TryExecute(new Dictionary<string, VariableContainer> { { "@TGT", new VariableContainer<ActionUnion>(roleUnion) } }, handler);
	}

	public void SetOrderable(bool v)
	{
		orderDragHandle.Visible = v;
	}

	// TODO: Implement Player custom sorting
	/*public void LiftOffOnDragBegin()
	{

	}

	public void FollowOnDrag(PointerEventData eventData)
	{

	}

	public void LandOnDragEnd()
	{
		int currentIndex = GetIndex();
		playerCardsSorter.MovePlayerInCustomOrder(lateSiblingIndex, currentIndex);
		lateSiblingIndex = currentIndex;
	}*/

	// TODO: Implement Player highlighting; Makes sense to call GameStateManager.UnobscurePlayersList here.
	public void HighlightPlayerCard()
	{
		/*StopAllCoroutines();
		StartCoroutine(BlinkImage());*/
	}

	private IEnumerator BlinkImage()
	{
		throw new NotImplementedException();
		/*float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			image.color = Color.Lerp(originalColor, originalColor * 2, t);
			yield return null;
		}

		t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime;
			image.color = Color.Lerp(originalColor * 2, originalColor, t);
			yield return null;
		}*/
	}

	private readonly Dictionary<string, VariableContainer> sharedVariables = new();
	void IScopeOwner.CreateSharedVariable(string variableName, VariableContainer defaultValue)
	{
		sharedVariables.Add(variableName, defaultValue);
	}

	void IScopeOwner.WriteSharedVariable(string variableName, VariableContainer data)
	{
		sharedVariables[variableName] = data;
	}

	VariableContainer IScopeOwner.ReadSharedVariable(string varName)
	{
		return sharedVariables[varName];
	}

	string IScopeOwner.GetSpecifiedOwnerName()
	{
		return PlayerName + " (PLAIN-TEXT_PLAYER)";
	}

	void IScopeOwner.SetCompromised()
	{
		throw new NotImplementedException(); // TODO
		/*playerRoleLabel.color = new Color(255, 190, 154);
		IsRoleActive = false;*/
	}

	public override void _ExitTree()
	{
		exitCancellationTokenSource.Cancel();
	}

    public override string ToString()
    {
        return PlayerName;
    }

	event Action IStringSource.OnStringChanged
	{
		add
		{
			PlayerNameChanged += value;
		}

		remove
		{
			PlayerNameChanged -= value;
		}
	}

	string IStringSource.GetString()
    {
        return PlayerName;
    }
}