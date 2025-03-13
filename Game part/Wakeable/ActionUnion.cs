using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace MafiaHostAssistant;

public partial class ActionUnion : Wakeable, IScopeOwner, IStringSource
{
	public string UnionName { get; private set; }
	public List<Player> CachedPlayersList { get; } = new();
	public List<Wakeable> ContainedWakeables { get; } = new();
	private PersistentScope waScope;
	private GameStateManager gameManager;
	private GameJournalManager journalManager;
	private bool isCompromised;
	private ConfiguredBehaviorRecord heldWaRef;
	private readonly CancellationTokenSource exitCancelationTokenSource = new();

	public event Action UnionNameChanged; // TODO: Keep in mind this existss

    public void SetUp(UnionOrderEntry orderEntry)
	{
		UnionName = orderEntry.unionName;
		heldWaRef = orderEntry.wakingAlgorythmRef;

		foreach (OrderEntry entry in orderEntry.entries)
		{
			if (entry is ActionOrderEntry actionEntry)
			{
				ContainedWakeables.AddRange(gameManager.FindActionsByOrderEntry(actionEntry));
			}
			if (entry is UnionOrderEntry unionEntry)
			{
				ActionUnion union = new();
				union.SetUp(unionEntry);
				ContainedWakeables.Add(union);
			}
			else if (entry is CombinationOrderEntry combinationEntry)
			{
				// TODO;
			}
		}

		CachePlayersRecursively(ContainedWakeables);

		void CachePlayersRecursively(List<Wakeable> wakeables)
		{
			foreach (Wakeable wakeable in wakeables)
			{
				if (wakeable is RoleActiveAction action)
				{
					if (!CachedPlayersList.Contains(action.Player))
					{
						CachedPlayersList.Add(action.Player);
					}
				}
				else if (wakeable is ActionUnion union)
				{
					CachePlayersRecursively(union.ContainedWakeables);
				}
				else if (wakeable is WakeableCombination combination)
				{
					CachePlayersRecursively(combination.ContainedWakeables);
				}
			}
		}
	}

	public override async Task Initialize()
	{
		BehaviorRecord waInfo = heldWaRef.ToBehaviorRecord(BehaviorType.WakingAlgorythm);
		waScope = new();
		await waScope.Initialize(
			behaviorInfo: waInfo,
			config: heldWaRef.config,
			behaviorName: heldWaRef.behaviorName,
			behaviorType: BehaviorType.WakingAlgorythm,
			scopeOwner: this,
			exitCancellationToken: exitCancelationTokenSource.Token
		);

		foreach (Wakeable wakeable in ContainedWakeables)
		{
			await wakeable.Initialize();
		}
	}

	public override async Task<bool> CanExecute(WakeableHandlerWindow handler)
	{
		return isCompromised || await waScope.TryExecute(null, handler);
	}

	private List<Wakeable> uniqueWakeables;
	private List<string> displayStrings;
	private int option;
	private WakeableHandlerWindow handler;

	public override void PrepareExecution(WakeableHandlerWindow handler)
	{
		uniqueWakeables = new(); // For options; If the size is 1, the union is simple
		displayStrings = new();
		option = -1;
		this.handler = handler;

		foreach (Wakeable wakeable in ContainedWakeables)
		{
			if (wakeable is RoleActiveAction action)
			{
				if (uniqueWakeables.Find(w => wakeable is RoleActiveAction a && a.ActionFileName == action.ActionFileName) != null)
				{
					uniqueWakeables.Add(action);
					displayStrings.Add(action.IsTraceless ? action.DisplayName : $"{action.DisplayName} ({action.Player.PlayerName})");
				}
				else if (!action.IsTraceless)
				{
					uniqueWakeables.Add(action);
					displayStrings.Add($"{action.DisplayName} ({action.Player.PlayerName})");
				}
			}
			else if (wakeable is ActionUnion actionUnion)
			{
				uniqueWakeables.Add(actionUnion);
				displayStrings.Add(actionUnion.UnionName);
			} // TODO: Combinations
		}

		if (uniqueWakeables.Count > 1)
		{
			// TODO: "null" is not a translation key.
			handler.CreateRouteSelectionField(TranslationServer.Translate(null), displayStrings, i => option = i);
		}
		else
		{
			option = 0;
		}
	}

	public override bool CanProceedToExecution()
	{
		return option != -1;
	}

	public override async Task ProceedToExecution()
	{
		await handler.HandleNestedWakeable(uniqueWakeables[option]);
	}

	public override void CancelBeforeExecution()
	{
		throw new NotImplementedException();
	}

	void IScopeOwner.CreateSharedVariable(string variableName, VariableContainer defaultValue)
	{
		throw new NotImplementedException();
	}

	void IScopeOwner.WriteSharedVariable(string variableName, VariableContainer data)
	{
		throw new NotImplementedException();
	}

	VariableContainer IScopeOwner.ReadSharedVariable(string valueName)
	{
		throw new NotImplementedException();
	}

	string IScopeOwner.GetSpecifiedOwnerName()
	{
		throw new NotImplementedException();
	}

	void IScopeOwner.SetCompromised()
	{
		isCompromised = true;
		waScope = null;
	}

	event Action IStringSource.OnStringChanged
	{
		add
		{
			UnionNameChanged += value;
		}

		remove
		{
			UnionNameChanged -= value;
		}
	}

	string IStringSource.GetString()
    {
        return UnionName;
    }

}
