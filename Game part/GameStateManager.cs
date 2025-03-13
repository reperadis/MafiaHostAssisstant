using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

// Manages the "Game" state of the "Game" scene, not the "Game" scene itself
public partial class GameStateManager : Control
{
	[Export] private Control windowsContent;
	[Export] private Control windowsTray; // Collapsed windows can be expanded by clicking on an button there
	[Export] private PackedScene notificationScene;
	[Export] private Control playerListContent;
	[Export] private PackedScene playerCardScene;
	[Export] private PackedScene listPlayerFieldScene;
	[Export] private GameJournalManager journalManager;

	private List<GameWindow> activeWindows;
	private readonly CancellationTokenSource exitCancelationTokenSource = new();
	public List<Player> Players { get; } = new(); // TODO: Force each name to be unique
	
	public async void SetUp(List<(string, RoleRecord)> nameRolePairs)
	{
		foreach ((string playerName, RoleRecord playerRole) in nameRolePairs)
		{
			Player playerCard = playerCardScene.Instantiate<Player>();
			playerListContent.AddChild(playerCard);
			Players.Add(playerCard);
			playerCard.SetUp(this, playerName, playerRole);
		}

		foreach (OrderEntry item in RoleList.orderEntries)
		{
			if (item is UnionOrderEntry unionEntry)
			{
				ActionUnion union = new();
				union.SetUp(unionEntry);
				RootWakeables.Add(union);
			}
			else if (item is ActionOrderEntry actionEntry)
			{
				RootWakeables.AddRange(FindActionsByOrderEntry(actionEntry));
			}
			else if (item is CombinationOrderEntry combinationEntry)
			{
				// TODO
			}
		}

		foreach (Player player in Players)
		{
			await player.Initialize();
		}
		foreach (Wakeable wakeable in RootWakeables)
		{
			await wakeable.Initialize();
		}
	}

	public List<RoleActiveAction> FindActionsByOrderEntry(ActionOrderEntry entry)
	{
		List<RoleActiveAction> res = new();
		foreach (Player player in Players)
		{
			if (player.roleInfo.roleName != entry.roleName)
			{
				continue;
			}
			res.Add(player.RoleActions[entry.actionIndex]);
		}
		return res;
	}

	[Export] private PackedScene wakeableHandlerWindowScene;
	[Export] private Control wakeableHandlersContent;
	[Export] private PackedScene subWakeablesHandlerWindowScene;
	[Export] private PackedScene tabButtonScene; // A normal button with set size and Toggle mode on;
	[Export] private Control tabButtonsContent;
	private Wakeable currentWakeable;
	private uint nightCount = 1;
	public bool NightInProgress { get; private set; } = false;
	public List<Wakeable> RootWakeables { get; } = new();

	public async void StartNight() // Only via the button
	{
		if (NightInProgress) return;
		NightInProgress = true;

		foreach (Wakeable wakeable in RootWakeables)
		{
			if (!IsInsideTree()) // If the "Game" scene was exited
			{
				return;
			}
			// TODO: No logs are being written currently, should be Wakeables' duty
			await HandleWakeables(wakeable);
		}
		NightInProgress = false;
		nightCount++;
	}

	public async Task HandleWakeables(params Wakeable[] wakeables)
	{
		// TODO: Display a path to the currently executed wakeable/s, e.g: "Mafia (Union) -> Mafia, Kill"; Should be at the top; update it whenever the windows are changed with the tab buttons
		foreach (Wakeable wakeable in wakeables)
		{
			Button tabButton = tabButtonScene.Instantiate<Button>();
			tabButtonsContent.AddChild(tabButton);
			WakeableHandlerWindow handler = wakeableHandlerWindowScene.Instantiate<WakeableHandlerWindow>();
			wakeableHandlersContent.AddChild(handler); // TODO: Tab button should have a text;
			tabButton.Toggled += v => ChangeWindow(tabButton.GetIndex(), v);
			await handler.HandleWakeable(wakeable, this);
		}
	}

	private int currentTabIndex; // Indexes of the tab buttons and their controls match; default is zero, which is the players list
	private void ChangeWindow(int tabIndex, bool toggledOn)
	{
		if (!toggledOn)
		{
			// TODO: Switch to players list; If it is the players list, force the button to be toggled
		}
		else
		{
			tabButtonsContent.GetChild<Button>(currentTabIndex).SetPressedNoSignal(false);
			wakeableHandlersContent.GetChild<Control>(currentTabIndex).Visible = false;
			wakeableHandlersContent.GetChild<Control>(tabIndex).Visible = toggledOn;
		}
		currentTabIndex = tabIndex;
	}

	public void UnobscurePlayersList()
	{
		// TODO: Night Journal is a windows, and so is Role Popup Window and many other planned ones; Keep a track of them or something;
		// TODO: Remove the TODO above and do like so: If journal is open, close it, if current tab is not player list, force it to be the player list, if NotificationWindow is open, collapse it. No other windows are planned;
	}

	public PlayersSortMode PlayersSortMode { get; private set; }
	public List<Player> CustomSortingOrder { get; private set; }
	public event Action SortOrderChanged; // TODO: Make Player Selection Fields listen to this. Sorting the Players List (the zeroth tab) is this manager's responsibility;

	public void SortPlayersByNames()
	{
		// TODO;
	}

	public override void _ExitTree()
	{
		exitCancelationTokenSource.Cancel();
	}
}

public enum PlayersSortMode
{
	CustomOrder,
	AlphabeticalyByNames,
	AlphabeticalyByRoles
}