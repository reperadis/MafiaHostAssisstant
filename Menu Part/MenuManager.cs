using Godot;
using Newtonsoft.Json;
using System.IO;

namespace MafiaHostAssistant;

public partial class MenuManager : Control
{
	[Export] private Control mainMenu;
	[Export] private Control deck;
	[Export] private Control behaviorsList;
	[Export] private Control behaviorsContent;
	[Export] private Control createMenu;
	[Export] private Control createMenuCT;
	[Export] private Control firstDeckPage;
	[Export] private Control secondDeckPage;
	[Export] private PackedScene behaviorCardScene;
	[Export] private Theme lightTheme;
	[Export] private Theme darkTheme;


	public override void _Ready()
	{		
		/*foreach (string path in Directory.EnumerateFiles(BehaviorType.WakingAlgorythm.GetFolderPath()))
		{
			BehaviorCard card = behaviorCardScene.Instantiate<BehaviorCard>();
			card.SetUp(JsonConvert.DeserializeObject<BehaviorInfo>(File.ReadAllText(path), GSSC.GSS), Path.GetFileNameWithoutExtension(path));
			behaviorsContent.AddChild(card);
		}

		foreach (string path in Directory.EnumerateFiles(BehaviorType.ActionWithPlayer.GetFolderPath()))
		{
			BehaviorCard card = behaviorCardScene.Instantiate<BehaviorCard>();
			card.SetUp(JsonConvert.DeserializeObject<BehaviorInfo>(File.ReadAllText(path), GSSC.GSS), Path.GetFileNameWithoutExtension(path));
			behaviorsContent.AddChild(card);
		}

		foreach (string path in Directory.EnumerateFiles(BehaviorType.ActionWithUnion.GetFolderPath()))
		{
			BehaviorCard card = behaviorCardScene.Instantiate<BehaviorCard>();
			card.SetUp(JsonConvert.DeserializeObject<BehaviorInfo>(File.ReadAllText(path), GSSC.GSS), Path.GetFileNameWithoutExtension(path));
			behaviorsContent.AddChild(card);
		}*/ // TODO: Uncomment

		Theme = Settings.Instance.IsLightTheme.Value ? lightTheme : darkTheme;
		Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);
	}

	private void OnThemeChanged(bool newIsLightTheme)
	{
		Theme = newIsLightTheme ? lightTheme : darkTheme;
	}

	public void LoadGame()
	{

	}

	private bool isOnFirstDeckPage = true;
	// TODO: Change buttons' icons when moving between deck pages
	public void OnLeftDeckButton()
	{
		if (isOnFirstDeckPage) // Move to main
		{
			Tween tween1 = mainMenu.CreateTween();
			tween1.TweenProperty(mainMenu, "position:y", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
			Tween tween2 = deck.CreateTween();
			tween2.TweenProperty(deck, "position:y", GetViewportRect().Size.Y, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		}
		else // Move to first
		{
			Tween tween1 = firstDeckPage.CreateTween();
			tween1.TweenProperty(firstDeckPage, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
			Tween tween2 = secondDeckPage.CreateTween();
			tween2.TweenProperty(secondDeckPage, "position:x", GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
			isOnFirstDeckPage = true;
		}
	}

	public void OnRightDeckButton()
	{
		if (isOnFirstDeckPage) // Move to second
		{
			Tween tween1 = firstDeckPage.CreateTween();
			tween1.TweenProperty(firstDeckPage, "position:x", -GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
			Tween tween2 = secondDeckPage.CreateTween();
			tween2.TweenProperty(secondDeckPage, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
			isOnFirstDeckPage = false;
		}
		else
		{
			// TODO: "Add Union" functionality goes there
		}
	}

	public void MoveFromMainToDeck() // Button in main menu
	{
		Tween tween1 = mainMenu.CreateTween();
		tween1.TweenProperty(mainMenu, "position:y", -GetViewportRect().Size.Y, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		Tween tween2 = deck.CreateTween();
		tween2.TweenProperty(deck, "position:y", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
	}
	
	public void MoveFromBehsToMain()
	{
		Tween tween1 = mainMenu.CreateTween();
		tween1.TweenProperty(mainMenu, "position:x", GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		Tween tween2 = behaviorsList.CreateTween();
		tween2.TweenProperty(behaviorsList, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
	}
	
	public void MoveFromMainToBehs()
	{
		Tween tween1 = mainMenu.CreateTween();
		tween1.TweenProperty(mainMenu, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		Tween tween2 = behaviorsList.CreateTween();
		tween2.TweenProperty(behaviorsList, "position:x", -GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
	}
	
	bool isTweeningCreateMenu;
	public void OpenCreateMenu()
	{
		if (isTweeningCreateMenu)
		{
			return;
		}
		isTweeningCreateMenu = true;
		Tween tween = createMenuCT.CreateTween();
		createMenuCT.Visible = true;
		tween.TweenProperty(createMenuCT, "color", new Color(0.165f, 0.165f, 0.165f, 0.78f), 0.2d);

		Tween tween1 = createMenu.CreateTween();
		tween1.TweenProperty(createMenu, "position:x", GetViewportRect().Size.X - 650, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		tween1.Finished += () => isTweeningCreateMenu = false;
	}
	
	public void CloseCreateMenu()
	{
		if (isTweeningCreateMenu)
		{
			return;
		}
		isTweeningCreateMenu = true;
		Tween tween = createMenuCT.CreateTween();
		tween.TweenProperty(createMenuCT, "color", new Color(0.165f, 0.165f, 0.165f, 0), 0.2d);
		tween.Finished += () => createMenuCT.Visible = false;

		Tween tween1 = createMenu.CreateTween();
		tween1.TweenProperty(createMenu, "position:x", GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
		tween1.Finished += () => isTweeningCreateMenu = false;
	}

	public void LoadRoleEditor()
	{
		// TODO;
	}

	public void LoadBehEditorForWakingAlgorythm()
	{
		BehaviorEditor.SetEditedType(BehaviorType.WakingAlgorythm);
		GetTree().ChangeSceneToFile(@"res://Scenes/Behavior Editor.tscn");
	}

	public void LoadBehEditorForActiveActionWithPlayer()
	{
		BehaviorEditor.SetEditedType(BehaviorType.ActiveActionWithPlayer);
		GetTree().ChangeSceneToFile(@"res://Scenes/Behavior Editor.tscn");
	}

	public void LoadBehEditorForPassiveActionWithPlayer()
	{
		BehaviorEditor.SetEditedType(BehaviorType.PassiveActionWithPlayer);
		GetTree().ChangeSceneToFile(@"res://Scenes/Behavior Editor.tscn");
	}

	public void LoadBehEditorForPassiveActionWithUnion()
	{
		BehaviorEditor.SetEditedType(BehaviorType.PassiveActionWithUnion);
		GetTree().ChangeSceneToFile(@"res://Scenes/Behavior Editor.tscn");
	}

	#region Role Card Displaying

	[Export] private Control cardsContent;
	[Export] private Control createNewRoleButton;
	[Export] private PackedScene roleCardScene;

    private void DisplayCards()
	{
		foreach (string filePath in Directory.EnumerateFiles(Path.Combine(OS.GetUserDataDir(), "Role Infos")))
		{
			RoleRecord roleInfo = JsonConvert.DeserializeObject<RoleRecord>(File.ReadAllText(filePath));
			RoleCard roleCard = (RoleCard)roleCardScene.Instantiate();
			roleCard.SetRoleRecord(roleInfo);
			cardsContent.AddChild(roleCard);
		}
		createNewRoleButton.MoveToFront();
	}
	
	#endregion
}
