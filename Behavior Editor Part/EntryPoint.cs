using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MafiaHostAssistant;

public sealed partial class EntryPoint : Node
{
	[Export] private Label nameLabel;
	[Export] private TextureRect returnTypeTextureRect;
	[Export] private TextureRect buttonTexture;
	[Export] private Texture2D lightTexture;
	[Export] private Texture2D darkTexture; // TODO: Not set yet
	[Export] private Node errorsContent;
	[Export] private PackedScene errorGroupScene;

	[Export] private BehaviorEditor behaviorEditor;
	[Export] private BEDocumentationWindow docsWindow;
	[Export] private Control behaviorWindow;
	[Export] private Control selectionManipulationTab;
	[Export] private OperationScope behaviorScope;

	public bool MustExitWithBool { get; private set; }
	private readonly Dictionary<Operation, ErrorMessageGroup> activeErrors = new();
	private readonly List<Operation> selectedOperations = new();

	public void SetUp(string entryPointName, bool mustExitWithBool)
	{
		nameLabel.Text = entryPointName;
		MustExitWithBool = mustExitWithBool;
		returnTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(mustExitWithBool ? BehaviorVariableType.Bool : BehaviorVariableType.Nothing);
		Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);
		behaviorScope.SetUp(null, null, mustExitWithBool, this, behaviorEditor);
	}

	private void OnThemeChanged(bool _)
	{
		returnTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(MustExitWithBool ? BehaviorVariableType.Bool : BehaviorVariableType.Nothing);
	}

	public void OpenBehWindow() // Button
	{
		behaviorWindow.Visible = true;
	}

	public void CloseBehWindow() // Button
	{
		behaviorWindow.Visible = false;
	}
	
	public int PushError(Operation erroringOperation, string[] docsPath, string message, bool isSaveBlocking)
	{
		ErrorMessageGroup msg;
		if (!activeErrors.ContainsKey(erroringOperation))
		{
			msg = errorGroupScene.Instantiate<ErrorMessageGroup>();
			errorsContent.AddChild(msg);
			msg.SetOperation(docsWindow, docsPath, erroringOperation);
			activeErrors.Add(erroringOperation, msg);
		}
		else
		{
			msg = activeErrors[erroringOperation];
		}
		return msg.AddError(message, isSaveBlocking);
	}
	
	public void ResolveError(Operation erroringOperation, int errorIndex)
	{
		if (activeErrors[erroringOperation].RemoveErrorAndDestroyIfFullyResolved(errorIndex))
		{
			activeErrors.Remove(erroringOperation);
		}
	}
	
	public void ResolveAllErrorsIfAny(Operation operation)
	{
		if (!activeErrors.ContainsKey(operation))
		{
			return;
		}
		activeErrors[operation].ResolveAllAndDestroy();
		activeErrors.Remove(operation);
	}

	public void SelectOperation(Operation operation)
	{
		selectedOperations.Add(operation);
		selectionManipulationTab.Visible = true;
	}

	public void DeselectOperation(Operation operation)
	{
		selectedOperations.Remove(operation);
		if (selectedOperations.Count == 0)
		{
			selectionManipulationTab.Visible = false;
		}
	}

	public void DeselectAllOperations()
	{
		foreach (Operation operation in selectedOperations)
		{
			operation.SetDeselected();
		}
		selectedOperations.Clear();
		selectionManipulationTab.Visible = false;
	}

	public void DeleteSelectedOperations()
	{
		foreach (Operation operation in selectedOperations)
		{
			operation.Delete();
		}
		selectedOperations.Clear();
		selectionManipulationTab.Visible = false;
	}

	public (List<OperationReference> actions, bool IsStateless) ReadEntryPoint()
	{
		return (behaviorScope.ReadScope(), behaviorScope.IsStateless());
	}

	public (bool hasErrors, bool isSaveBlocking) GetErrorsStatus()
	{
		return (activeErrors.Count != 0, activeErrors.Values.Any(e => e.IsSaveBlocking()));
	}
}