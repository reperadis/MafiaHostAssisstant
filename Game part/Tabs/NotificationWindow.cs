using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class NotificationWindow : Control
{
	[Export] private GameStateManager gameManager;
	[Export] private Control messagePanel;
	[Export] private RuntimeDynamicStringLabel messageLabel;
	[Export] private Control buttonsPanel;
	[Export] private Control buttonsContent;
	[Export] private Control fieldContainer;
	[Export] private HoldableButton confirmButton;
	[Export] private PackedScene hButtonScene;
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene intFieldScene;
	[Export] private PackedScene stringFieldScene;
	[Export] private PackedScene playerFieldScene;
	[Export] private PackedScene listBoolFieldScene;
	[Export] private PackedScene listIntFieldScene;
	[Export] private PackedScene listStringFieldScene;
	private TaskCompletionSource completionSource;

	/*public void SetUp(string ownerName, string behName, string behaviorSpecification, List<DynamicStringElementData> message)
	{
		// TODO: Make sure that whatever calls this method converts the message from variable names to their values.
		Visible = true;
		if (message == null || message.Count == 0) return;
		messagePanel.Visible = true;
		messageLabel.SetUp(gameManager);
		messageLabel.SetText(message);
	}*/

	public async Task<int> AskOption(List<string> optionTexts)
	{
		int res = 0;
		List<HoldableButton> buttons = new(optionTexts.Count);
		completionSource = new TaskCompletionSource();
		foreach (string text in optionTexts)
		{
			HoldableButton button = hButtonScene.Instantiate<HoldableButton>();
			button.Text = text;
			buttonsContent.AddChild(button);
			buttons.Add(button);
			button.Held += () => { res = button.GetIndex(); completionSource.SetResult(); };
		}

		buttonsPanel.Visible = true;
		await completionSource.Task;
		return res;
	}

	public async Task<bool> AskBoolValue(bool current, string fieldLabel)
	{
		bool res = current;
		NamedBoolConfigField field = boolFieldScene.Instantiate<NamedBoolConfigField>();
		field.SetUp(fieldLabel, current, v => res = v);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
		return res;
	}

	public async Task<int> AskIntValue(int current, string fieldLabel)
	{
		int res = current;
		NamedIntConfigField field = intFieldScene.Instantiate<NamedIntConfigField>();
		field.SetUp(fieldLabel, current, true, v => res = v);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
		return res;
	}

	public async Task<string> AskStringValue(string current, string fieldLabel)
	{
		string res = current;
		NamedStringConfigField field = stringFieldScene.Instantiate<NamedStringConfigField>();
		field.SetUp(fieldLabel, current, StringFieldContext.Unrestricted, false, v => res = v);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
		return res;
	}

	/*public async Task<Player> AskPlayerValue(Player current, string fieldLabel)
	{
		Player res = current;
		NamedPlayerSelectionField field = playerFieldScene.Instantiate<NamedPlayerSelectionField>();
		field.SetUp(fieldLabel, current, gameManager, v => res = v);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
		return res;
	}*/

	/*public async Task<ActionUnion> AskUnionValue(ActionUnion current, string fieldLabel)
	{
		ActionUnion res = current;
		NamedUnionSelectionField field = unionFieldScene.Instantiate<NamedUnionSelectionField>();
		field.SetUp(fieldLabel, current, gameManager, v => res = v);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
		return res;
	}*/

	public async Task AskListOfBoolsValue(List<bool> current, string fieldLabel)
	{
		List<bool> res = current;
		NamedListBoolConfigField field = listBoolFieldScene.Instantiate<NamedListBoolConfigField>();
		field.SetUp(fieldLabel, current, null);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
	}

	public async Task AskListOfIntsValue(List<int> current, string fieldLabel)
	{
		List<int> res = current;
		NamedListIntConfigField field = listIntFieldScene.Instantiate<NamedListIntConfigField>();
		field.SetUp(fieldLabel, current, null);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
	}

	public async Task AskListOfStringsValue(List<string> current, string fieldLabel)
	{
		List<string> res = current;
		NamedListStringConfigField field = listStringFieldScene.Instantiate<NamedListStringConfigField>();
		field.SetUp(fieldLabel, current, null);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
	}

	/*public async Task AskListOfPlayersValue(List<Player> current, string fieldLabel)
	{
		List<Player> res = current;
		NamedListPlayerSelectionField field = listPlayerFieldScene.Instantiate<NamedListPlayerSelectionField>();
		field.SetUp(fieldLabel, current, gameManager, null);
		fieldContainer.AddChild(field);
		field.label.AutoTranslateMode = AutoTranslateModeEnum.Disabled;

		confirmButton.Visible = true;
		completionSource = new TaskCompletionSource();
		await completionSource.Task;
	}*/

	public void Confirm() // Confirm H-button
	{
		completionSource.SetResult();
		QueueFree();
	}

	public void Clear()
	{
		buttonsPanel.Visible = false;
		foreach (Node node in buttonsContent.GetChildren())
		{
			node.QueueFree();
		}
		messageLabel.Visible = false;
		messageLabel.SetText(new()); // TODO: Is it realy neccessary to set the text to nothing? Doesn't setting it to the new one when needed suffice?
		confirmButton.Visible = false;
	}
}
