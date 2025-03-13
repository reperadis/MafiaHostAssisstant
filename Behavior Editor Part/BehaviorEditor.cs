using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace MafiaHostAssistant;

public partial class BehaviorEditor : Control
{
	private static bool IsEditing = false; // True if not creating a new Behavior, but editing an existing one
	private static BehaviorType? EditedType;
	private static string EditedBehaviorName;
	private string behaviorName;

	[Export] private Theme lightTheme;
	[Export] private Theme darkTheme;

	[Export] private EntryPoint iniEntryPoint;
	[Export] private EntryPoint specEntryPoint;

	[Export] private OperationScope rootIniScope;
	[Export] private OperationScope rootSpecScope;
	[Export] private Control saveErrorsColorTint;
	[Export] private Control invalidBehaviorNameColorTint;
	[Export] private Label saveErrorDetailsLabel;
	[Export] private Button exitAnywayButton;

	// Cached to not load them all the time
	private Texture2D boolTypeSprite;
	private Texture2D intTypeSprite;
	private Texture2D stringTypeSprite;
	private Texture2D playerTypeSprite;
	private Texture2D unionTypeSprite;
	private Texture2D listOfBoolTypeSprite;
	private Texture2D listOfIntTypeSprite;
	private Texture2D listOfStringTypeSprite;
	private Texture2D listOfPlayerTypeSprite;
	private Texture2D nothingTypeSprite;
	
	// Variables that persist between entry point calls
	public readonly static BehaviorVariable NullVariable = new("@Null", BehaviorVariableType.Nothing, false) { IsInvalid = false };
	public readonly List<BehaviorVariable> GlobalVariables = new()
	{
		{ NullVariable },
		{ new ("@True", BehaviorVariableType.Bool, true) },
		{ new ("@False", BehaviorVariableType.Bool, true) }
	};
	public readonly List<BehaviorVariable> ConfigurableVariables = new();
	public readonly List<BehaviorVariable> AccessedSharedVariables = new();
	public readonly List<BehaviorVariable> CreatedSharedVariables = new();
	public Action<BehaviorVariable> OnVariableAddedOrRenamed;

	public static void SetEditData(string editedBehaviorName)
	{
		IsEditing = true;
		EditedBehaviorName = editedBehaviorName;
	}

	public static void SetEditedType(BehaviorType editedType)
	{
		EditedType = editedType;
	}
	
	public override void _Ready()
	{
		if (!EditedType.HasValue)
		{
			GD.PushError("Edited Type is somehow null");
			return;
		}
		behaviorName = EditedBehaviorName;

		Theme = Settings.Instance.IsLightTheme.Value ? lightTheme : darkTheme;
		Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);


		iniEntryPoint.SetUp(Tr("TK:INITIALIZATION"), false);
		
		switch (EditedType.Value)
		{
			case BehaviorType.ActiveActionWithPlayer:
				specEntryPoint.SetUp(Tr("TK:ACTION"), false);
				break;

			case BehaviorType.PassiveActionWithPlayer:
				BehaviorVariable pVariable = new("@TGT", BehaviorVariableType.Player, true);
				rootSpecScope.variables.Add(pVariable);
				rootSpecScope.OnVariableAddedOrRenamed?.Invoke(pVariable);
				specEntryPoint.SetUp(Tr("TK:ACTION"), false);
				break;

			case BehaviorType.WakingAlgorythm:
				specEntryPoint.SetUp(Tr("TK:EVALUATE"), true);
				break;

			case BehaviorType.PassiveActionWithUnion:
				BehaviorVariable uVariable = new("@TGT", BehaviorVariableType.Union, true);
				rootSpecScope.variables.Add(uVariable);
				rootSpecScope.OnVariableAddedOrRenamed?.Invoke(uVariable);
				specEntryPoint.SetUp(Tr("TK:ACTION"), false);
				break;
			default:
				break;
		}

		if (IsEditing)
		{
			BehaviorRecord codeInfo = null;
			switch (EditedType.Value)
			{
				case BehaviorType.ActiveActionWithPlayer:
					codeInfo = JsonConvert.DeserializeObject<BehaviorRecord>(File.ReadAllText(Path.Combine(FilePaths.GetActiveActionsWithPlayersDirectoryPath(), EditedBehaviorName + ".json")), GSSC.GSS);
					break;

				case BehaviorType.PassiveActionWithPlayer:
					codeInfo = JsonConvert.DeserializeObject<BehaviorRecord>(File.ReadAllText(Path.Combine(FilePaths.GetPassiveActionsWithPlayersDirectoryPath(), EditedBehaviorName + ".json")), GSSC.GSS);
					break;

				case BehaviorType.WakingAlgorythm:
					codeInfo = JsonConvert.DeserializeObject<BehaviorRecord>(File.ReadAllText(Path.Combine(FilePaths.GetWakingAlgorythmsDirectoryPath(), EditedBehaviorName + ".json")), GSSC.GSS);
					break;

				case BehaviorType.PassiveActionWithUnion:
					codeInfo = JsonConvert.DeserializeObject<BehaviorRecord>(File.ReadAllText(Path.Combine(FilePaths.GetPassiveActionsWithUnionsDirectoryPath(), EditedBehaviorName + ".json")), GSSC.GSS);
					break;

				default:
					break;
			}

			rootIniScope.WriteScope(codeInfo.IniSequence);
			rootSpecScope.WriteScope(codeInfo.MainSequence);
		}

		if (EditedType.Value == BehaviorType.ActiveActionWithPlayer)
		{
			createsLogField.Visible = true;
			createsLogField.SetUp(Tr("TK:BEHAVIOUR-CONFIG_CREATE-LOG"), createsLog, RegisterCreatesLog);
		}

		behaviorNameField.SetUp(Tr("TK:BEHAVIOR-NAME"), EditedBehaviorName ?? string.Empty, StringFieldContext.FileName, false, RegisterBehNameChange);
		tagsField.SetUp(JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(FilePaths.GetTagsTrackerFilePath())));
	}

	public void TrySave()
	{
		(bool iniHasErrors, bool iniSaveBlocking) = iniEntryPoint.GetErrorsStatus();
		(bool specHasErrors, bool specSaveBlocking) = specEntryPoint.GetErrorsStatus();

		if (string.IsNullOrWhiteSpace(behaviorName))
		{
			invalidBehaviorNameColorTint.Visible = true;
			return;
		}

		if (iniHasErrors || specHasErrors)
		{
			saveErrorsColorTint.Visible = true;
			if (iniSaveBlocking || specSaveBlocking)
			{
				saveErrorDetailsLabel.Text = Tr("TK:BEHAVIOR-CONTAINS-ERRORS_CANNOT-SAVE");
				exitAnywayButton.Text = Tr("TK:REVERT-CHANGES-AND-SAVE");
			}
			else
			{
				saveErrorDetailsLabel.Text = Tr("TK:BEHAVIOR-CONTAINS-ERRORS_CAN-SAVE");
				exitAnywayButton.Text = Tr("TK:SAVE-ANYWAY");
			}
			return;
		}
	}

	public void OnExitAnywaysButton()
	{
		(bool iniHasErrors, bool iniSaveBlocking) = iniEntryPoint.GetErrorsStatus();
		(bool specHasErrors, bool specSaveBlocking) = specEntryPoint.GetErrorsStatus();

		if (iniHasErrors || specHasErrors)
		{
			saveErrorsColorTint.Visible = true;
			if (iniSaveBlocking || specSaveBlocking)
			{
				ExitEditor();
			}
			else
			{
				CreateFile();
			}
		}
		else
		{
			CreateFile();
		}
	}

	public void CreateFile()
	{
		(List<OperationReference> iniActions, bool iniIsTraceless) = iniEntryPoint.ReadEntryPoint();
		(List<OperationReference> specActions, bool specIsTraceless) = specEntryPoint.ReadEntryPoint();

		if (behaviorName != EditedBehaviorName && File.Exists(EditedBehaviorName))
		{
			File.Delete(EditedBehaviorName);
		}
		EditedBehaviorName = behaviorName;

		BehaviorRecord behInfo = new()
		{
			ConfigurableVariables = ConfigurableVariables.Select(p => p.Deconstruct()).ToArray(),
			CreatedSharedVariables = CreatedSharedVariables.Select(p => p.Deconstruct()).ToArray(),
			AccessedSharedVariables = AccessedSharedVariables.Select(p => p.Deconstruct()).ToArray(),
			IniSequence = iniActions ?? null,
			MainSequence = specActions ?? null,

			CIBehaviorType = EditedType.Value,
			leavesNoTrace = iniIsTraceless && specIsTraceless,
			createsLog = createsLog,
		};

		string json = JsonConvert.SerializeObject(behInfo, GSSC.GSS);

		File.WriteAllText(Path.Combine(EditedType.Value.GetFolderPath(), EditedBehaviorName) + ".json", json);
		ExitEditor();
	}

	// TODO: Allow saving behavior with errors, but not allow using them until the errors are resolved.
	// Behavior with conflicting names should not be saveable, because loading variables back requires them having a unique name,
	// ParentScope.FindVariableByName() may give the wrong variable otherwise.
	public void ExitEditor()
	{
		GetTree().ChangeSceneToFile(@"res://Scenes/Menu.tscn");
		IsEditing = false;
		EditedType = null;
		EditedBehaviorName = string.Empty;
	}

	private void OnThemeChanged(bool newIsLightTheme)
	{
		Theme = newIsLightTheme ? lightTheme : darkTheme;
	}

	public Texture2D GetVarTypeTexture(BehaviorVariableType type)
	{
		return type switch
		{
			BehaviorVariableType.Bool => boolTypeSprite,
			BehaviorVariableType.Integer => intTypeSprite,
			BehaviorVariableType.String => stringTypeSprite,
			BehaviorVariableType.Player => playerTypeSprite,
			BehaviorVariableType.Union => unionTypeSprite,
			BehaviorVariableType.ListOfBools => listOfBoolTypeSprite,
			BehaviorVariableType.ListOfInts => listOfIntTypeSprite,
			BehaviorVariableType.ListOfStrings => listOfStringTypeSprite,
			BehaviorVariableType.ListOfPlayers => listOfPlayerTypeSprite,
			BehaviorVariableType.Anything => null,
			BehaviorVariableType.ListOfAnything => null,
			BehaviorVariableType.Nothing => nothingTypeSprite,
			_ => null,
		};
	}

	#region Operation Config Window

	[ExportGroup("Config Fields")]
	[Export] private PackedScene dynamicStringFieldScene;
	[Export] private PackedScene typeSearchFieldScene;
	[Export] private PackedScene beListFieldScene;
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene intFieldScene;
	[Export] private PackedScene stringFieldScene;
	[Export] private PackedScene enumFieldScene;

	[ExportGroup("")]
	[Export] private Node operationConfigWindowContent;
	[Export] private ColorRect operationConfigWindowCR;

	public void SetConfigWindowActive()
	{
		operationConfigWindowCR.Visible = true;
	}
	
	public void CloseConfigWindow()
	{
		foreach (Node field in operationConfigWindowContent.GetChildren())
		{
			field.QueueFree();
		}
		operationConfigWindowCR.Visible = false;
	}

	public NamedBoolConfigField CreateBoolField(string label, bool current, Action<bool> receivingFunc)
	{
		NamedBoolConfigField field = boolFieldScene.Instantiate<NamedBoolConfigField>();
		field.SetUp(label, current, receivingFunc);
		operationConfigWindowContent.AddChild(field);
		return field;
	}

	public NamedIntConfigField CreateIntField(string label, int current, bool allowNegative, Action<int> receivingFunc)
	{
		NamedIntConfigField field = intFieldScene.Instantiate<NamedIntConfigField>();
		field.SetUp(label, current, allowNegative, receivingFunc);
		operationConfigWindowContent.AddChild(field);
		return field;
	}

	public NamedStringConfigField CreateStringField(string label, string current, Action<string> receivingFunc)
	{
		NamedStringConfigField field = stringFieldScene.Instantiate<NamedStringConfigField>();
		field.SetUp(label, current, StringFieldContext.BehaviorVariableName, false, receivingFunc);
		operationConfigWindowContent.AddChild(field);
		return field;
	}

	public Control CreateEnumField(string label, IEnumerable<Dropdown.ElementData> options, int current, Action<int> receivingFunc)
	{
		NamedEnumConfigField field = enumFieldScene.Instantiate<NamedEnumConfigField>();
		field.SetUp(label, current, options, receivingFunc);
		operationConfigWindowContent.AddChild(field);
		return field;
	}

	public NamedTypeSearchField CreateTypeSearchField(string label, OperationScope scope, BehaviorVariableType requestedType, bool excludeSystemVariables, BehaviorVariable current, Action<BehaviorVariable> receivingFunc)
	{
		NamedTypeSearchField field = typeSearchFieldScene.Instantiate<NamedTypeSearchField>();
		operationConfigWindowContent.AddChild(field);
		field.SetUp(label, current, scope, requestedType, excludeSystemVariables, this, receivingFunc);
		return field;
	}
	
	public BEListConfigField CreateBEListField(string label, OperationScope scope, BehaviorVariableType containedType, List<BEListElementData> operatedList)
	{
		BEListConfigField field = beListFieldScene.Instantiate<BEListConfigField>();
		operationConfigWindowContent.AddChild(field);
		field.SetUp(label, operatedList, containedType, scope, this);
		return field;
	}

	public BEDynamicStringConfigField CreateBEDynamicStringField(string label, OperationScope scope, List<BEDynamicStringElementData> operatedList)
	{
		BEDynamicStringConfigField field = dynamicStringFieldScene.Instantiate<BEDynamicStringConfigField>();
		operationConfigWindowContent.AddChild(field);
		field.SetUp(label, scope, operatedList, this);
		return field;
	}
	#endregion

	#region Behavior Configuration
	[ExportGroup("Behvaiour Configuration")]
	[Export] private Control behaviorConfigurationCT;
	[Export] private NamedStringConfigField behaviorNameField;
	[Export] private NamedBoolConfigField createsLogField;
	[Export] private BEItemSelectionField tagsField;

	public bool createsLog = true;

	public void RegisterBehNameChange(string behName)
	{
		behaviorName = behName;
	}

	public void RegisterCreatesLog(bool value)
	{
		createsLog = value;
	}
	
	#endregion
	
	[ExportGroup("Operations")]

	[Export] private PackedScene branchingNotificationScene; // TODO: Implement the ones with 4 references;
	[Export] private PackedScene ifStatementScene;
	[Export] private PackedScene createPersistentVariableScene;
	[Export] private PackedScene createConfigScene;
	[Export] private PackedScene createSharedVariableScene;
	[Export] private PackedScene createVariableScene;
	[Export] private PackedScene readSharedVariableScene;
	[Export] private PackedScene attachLogNoteScene;
	[Export] private PackedScene askValueScene;
	[Export] private PackedScene assignDirectlyScene;
	[Export] private PackedScene controlFlowScene;
	[Export] private PackedScene foreachLoopScene;
	[Export] private PackedScene getValueScene;
	[Export] private PackedScene highlightPlayerScene;
	[Export] private PackedScene pushNotificationScene;
	
	public PackedScene GetOperationScene(OperationName operationName)
	{
		return operationName switch
		{
			OperationName.IfStatement => ifStatementScene,
			OperationName.CreatePersistentVariable => createPersistentVariableScene,
			OperationName.CreateConfig => createConfigScene,
			OperationName.CreateSharedVariable => createSharedVariableScene,
			OperationName.CreateVariable => createVariableScene,
			OperationName.ReadSharedVariable => readSharedVariableScene,
			OperationName.AttachLogNote => attachLogNoteScene,
			OperationName.AskValue => askValueScene,
			OperationName.AssignDirectly => assignDirectlyScene,
			OperationName.ControlFlow => controlFlowScene,
			OperationName.ForeachLoop => foreachLoopScene,
			OperationName.GetValue => getValueScene,
			OperationName.HighlightPlayer => highlightPlayerScene,
			_ => throw new Exception($"Unknown requested operation {operationName}"),
		};
	}
}
