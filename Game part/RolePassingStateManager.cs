using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class RolePassingStateManager : Control
{
	[Export] private Control showingState;
	[Export] private Control hiddenState;
	[Export] private LineEdit playerNameField;
	[Export] private Label roleNameText;
	[Export] private Label roleDescriptionText;

	private readonly List<(string playerName, RoleRecord playerRole)> nameRolePairs = new();
	private string currentPlayerName = string.Empty;
	private TaskCompletionSource<bool> source;
	private List<string> playerNames;

	public async void SetUp(List<string> playerNames)
	{
		this.playerNames = playerNames;

		HideRole();

		List<RoleRecord> roleHeap = new();

		foreach (KeyValuePair<RoleRecord, int> pair in RoleList.selectedRoles)
		{
			for (int i = 0; i < pair.Value; i++)
			{
				roleHeap.Add(pair.Key);
			}
		}

		{ // Shuffle
			Random rng = new();
			int n = roleHeap.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				(roleHeap[n], roleHeap[k]) = (roleHeap[k], roleHeap[n]);
			}
		}

		int playerNameIndex = 0;
		foreach (RoleRecord role in roleHeap)
		{
			if (playerNameIndex < playerNames.Count) // If there is still a player name available
			{
				currentPlayerName = playerNames[playerNameIndex];
			}
			else
			{
				currentPlayerName = "";
			}

			roleNameText.Text = role.roleName;
			roleDescriptionText.Text = role.roleDescription;
			playerNameField.Text = currentPlayerName;

			source = new();
			await source.Task;

			nameRolePairs.Add((currentPlayerName, role));
			playerNameIndex++;
		}
	}

	public void TryEndShowingState() // H-button in the showing state
	{
		if (currentPlayerName != "")
		{
			source.SetResult(true);
			HideRole();
		}
		else
		{
			// TODO: Hint the user to input a name
		}
	}

	public void ShowRole() // H-button in hidenState
	{
		showingState.Visible = true;
		hiddenState.Visible = false;
	}

	private void HideRole()
	{
		showingState.Visible = false;
		hiddenState.Visible = true;
	}

	public void CatchPlayerNameChange(string pName) // Text field above the window
	{
		currentPlayerName = pName;
	}

	public List<(string, RoleRecord)> GetNameRolePairs()
	{
		return nameRolePairs;
	}
}
