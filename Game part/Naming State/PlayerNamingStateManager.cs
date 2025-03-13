using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MafiaHostAssistant;

public partial class PlayerNamingStateManager : Control
{
	[Export] private Control nameCardsContent;
	[Export] private Control addCardButton;
	[Export] private Control namesIOOptionSelectionCT; // Colour Tilt
	[Export] private Control namesSavePanel;
	[Export] private Control namesLoadPanel;
	[Export] private Control namesLoadOptionsContent;
	[Export] private NamedStringConfigField namesSaveNameField;
	[Export] private PackedScene holdableButtonScene;
	[Export] private PackedScene playerNameCardScene;
	[Export] private PackedScene additionalPlayerNameCardScene;

	private string savedNameListName;
	private readonly List<string> pathLoadOptions = new();
	private int totalRoleCount;

	public override void _Ready()
    {
		namesSaveNameField.SetUp(Tr("TK:NAME"), string.Empty, StringFieldContext.FileName, false, CatchNameListName);

		foreach (string filePath in Directory.EnumerateFiles(FilePaths.GetPlayerNamePresetsDirectoryPath()))
		{
			pathLoadOptions.Add(filePath); // TODO: adjust size of 'btn'
			HoldableButton btn = holdableButtonScene.Instantiate<HoldableButton>();
			namesLoadOptionsContent.AddChild(btn);
			btn.Text = Path.GetFileNameWithoutExtension(filePath);
			btn.Held += () => LoadPlayerNames(btn.GetIndex());
		}

		foreach (KeyValuePair<RoleRecord, int> pair in RoleList.selectedRoles)
		{
			totalRoleCount += pair.Value;
		}
	}

    public void AddNameCard()
	{
		if (nameCardsContent.GetChildCount() - 1 >= totalRoleCount) // -1 to account for Add button
		{
			return;
		}
		PlayerNamingCard card = playerNameCardScene.Instantiate<PlayerNamingCard>();
		card.SetUp(this);
		nameCardsContent.AddChild(card);
		nameCardsContent.MoveChild(addCardButton, -1);
	}

	public void TryFocusOnNextNameCard(int currentCardChildIndex)
	{
		if (currentCardChildIndex == nameCardsContent.GetChildCount() - 2)
		{
			return;
		}
		PlayerNamingCard card = nameCardsContent.GetChild<PlayerNamingCard>(currentCardChildIndex + 1);
		card.nameLineEdit.CallDeferred(Control.MethodName.GrabFocus);
	}

	public void OpenNamesLoadPanel()
	{
		namesIOOptionSelectionCT.Visible = true;
		namesLoadPanel.Visible = true;
	}

	public void OpenNamesSavePanel()
	{
		namesIOOptionSelectionCT.Visible = true;
		namesSavePanel.Visible = true;
	}

	public void CloseNameIOPanel()
	{
		namesIOOptionSelectionCT.Visible = false;
		namesSavePanel.Visible = false;
		namesLoadPanel.Visible = false;
	}

	public void SavePlayerNames() // TODO: Attach all the signals used for saving and loading names
	{
		if (string.IsNullOrWhiteSpace(savedNameListName) || File.Exists(Path.Combine(FilePaths.GetPlayerNamePresetsDirectoryPath(), savedNameListName)))
		{
			return;
		}

		List<string> names = nameCardsContent.GetChildren().Select(card => ((PlayerNamingCard)card).nameLineEdit.Text).ToList();

		File.WriteAllText(Path.Combine(FilePaths.GetPlayerNamePresetsDirectoryPath(), savedNameListName) + ".json", JsonConvert.SerializeObject(names));
	}

	public void LoadPlayerNames(int optionIndex)
	{
		foreach (Node card in nameCardsContent.GetChildren())
		{
			card.QueueFree();
		}

		List<string> loadedNames = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(pathLoadOptions[optionIndex]));

		foreach (string name in loadedNames)
		{
			PlayerNamingCard card = additionalPlayerNameCardScene.Instantiate<PlayerNamingCard>();
			nameCardsContent.AddChild(card);
			card.SetUp(this);
			card.nameLineEdit.Text = name;
		}
	}

	public void CatchNameListName(string name)
	{
		savedNameListName = name;
	}

	public List<string> GetPlayerNames()
	{
		return nameCardsContent.GetChildren().Select(c => ((PlayerNamingCard)c).nameLineEdit.Text).ToList();
	}
}
