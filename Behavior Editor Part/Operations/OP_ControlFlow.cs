using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_ControlFlow : Operation
{
	[Export] private Label label; // For changing name depending on Mode
	[Export] private Control returnGroup;
	[Export] private Label returnedVarNameLabel;
	[Export] private TextureRect returnedVarTypeTextureRect;
	[Export] private Control depthGroup;
	[Export] private Label depthLabel; // TODO: Don't forget to add a constant icon to the depth field

	private readonly List<Dropdown.ElementData> visibleModeOptions = new();
	private readonly List<ControlMode> avaibleModes = new();
	private int maxAllowedExitScopeDepth;
	private ControlMode currentMode = ControlMode.StopProcess;
	private int currentModeIndex = 0;
	private BehaviorVariableHandler returnedVariableHandler;
	private int flowControlDepth; // Setting to 1 breaks/escapes the innersmost scope from perspective of the operation, setting to 2 does so to the innermost scope and its parent scope.
	private int badDepthErrorIndex = -1;

	private BehaviorEditor behaviorEditor;

	private Control additionalField = null; // Either a Type Search or an Int (depth) field

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		// Modes not being localised is intended
		visibleModeOptions.Add(new Dropdown.ElementData(null, Tr("TK:OP_FIELD_MODE_STOP-PROCESS")));
		avaibleModes.Add(ControlMode.StopProcess);

		Operation current = ParentScope.ParentOperation;
		while (current != null)
		{
			if (current is OP_ForeachLoop or null)
			{
				break;
			}
			maxAllowedExitScopeDepth++;
			current = current.ParentScope.ParentOperation;
		}

		if (ParentScope.MustExitWithBool)
		{
			returnedVariableHandler = new BehaviorVariableHandler(behaviorEditor, this, Tr("TK:OP_FIELD_RETURN"), BehaviorVariableType.Bool, returnedVarNameLabel, returnedVarTypeTextureRect);
			returnGroup.Visible = true;
		}

		if (ParentScope.ParentCodeScope == null) // If in the root
		{
			return;
		}
		if (ParentScope.CycleLevel != 0)
		{
			visibleModeOptions.Add(new Dropdown.ElementData(null, Tr("TK:OP_FIELD_MODE_STOP-CYCLE")));
			visibleModeOptions.Add(new Dropdown.ElementData(null, Tr("TK:OP_FIELD_MODE_SKIP-ITERATION")));
			avaibleModes.Add(ControlMode.StopCycle);
			avaibleModes.Add(ControlMode.SkipIteration);
		}
		if (ParentScope.GetParent() is not OP_ForeachLoop)
		{
			visibleModeOptions.Add(new Dropdown.ElementData(null, Tr("TK:OP_FIELD_MODE_EXIT-SCOPE")));
			avaibleModes.Add(ControlMode.ExitScope);
		}
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		behaviorEditor.CreateEnumField(Tr("TK:OP_FIELD_MODE"), visibleModeOptions, currentModeIndex, RecieveMode);
		CreateAdditionalField();
	}

	private void RecieveMode(int modeIndex)
	{
		if (currentModeIndex == modeIndex && additionalField != null)
		{
			return;
		}

		currentModeIndex = modeIndex;
		currentMode = avaibleModes[modeIndex];
	}

	private void RecieveControlDepth(int value)
	{
		flowControlDepth = value;
		returnedVarNameLabel.Text = value.ToString();
		if ((currentMode == ControlMode.StopCycle || currentMode == ControlMode.SkipIteration) && flowControlDepth > ParentScope.CycleLevel)
		{
			if (badDepthErrorIndex != -1)
			{
				ResolveError(badDepthErrorIndex); // TODO: Path is temporarily NULL
				badDepthErrorIndex = PushError(null, ConstructDepthCantExceedLimitError(flowControlDepth, ParentScope.CycleLevel), false);
			}
			return;
		}
		if (currentMode == ControlMode.ExitScope && flowControlDepth > maxAllowedExitScopeDepth)
		{
			if (badDepthErrorIndex != -1)
			{
				ResolveError(badDepthErrorIndex); // TODO: Path is temporarily NULL
				badDepthErrorIndex = PushError(null, ConstructDepthCantExceedLimitError(flowControlDepth, maxAllowedExitScopeDepth), false);
			}
			return;
		}
		if (badDepthErrorIndex != -1)
		{
			ResolveError(badDepthErrorIndex);
			badDepthErrorIndex = -1;
		}
	}

	private void CreateAdditionalField()
	{
		if (currentMode == ControlMode.StopProcess)
		{
			if (additionalField != null && IsInstanceValid(additionalField))
			{
				additionalField.QueueFree();
				additionalField = null;
			}
			depthGroup.Visible = false;
			flowControlDepth = 1;
			depthLabel.Text = "1";
			if (ParentScope.MustExitWithBool)
			{
				additionalField = returnedVariableHandler.CreateSelectionField(false);
				returnGroup.Visible = true;
			}
			if (badDepthErrorIndex != -1)
			{
				ResolveError(badDepthErrorIndex);
				badDepthErrorIndex = -1;
			}
			return;
		}
		returnGroup.Visible = false;
		if (currentMode == ControlMode.StopCycle || currentMode == ControlMode.ExitScope || currentMode == ControlMode.SkipIteration)
		{
			depthGroup.Visible = true;
			additionalField?.QueueFree();
			additionalField = behaviorEditor.CreateIntField(Tr("TK:OP_FIELD_CONTROL-DEPTH"), flowControlDepth, false, RecieveControlDepth);
		}
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.ControlFlow, new Arguments(currentMode, returnedVariableHandler.Variable.TrueVariableName, flowControlDepth));
	}
	
	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		flowControlDepth = args.flowControlDepth;
		currentMode = args.controlMode;
		currentModeIndex = avaibleModes.FindIndex(m => m == currentMode);

		if (ParentScope.MustExitWithBool)
		{
			returnedVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.returnVarName));
		}
	}

	public override string GetReadableOpearationName()
	{
		return visibleModeOptions[currentModeIndex].label;
	}

    public override bool IsStateless => true;

    [Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public ControlMode controlMode;
		public string returnVarName;
		public int flowControlDepth;
		public Arguments(ControlMode controlMode, string returnVarName, int flowControlDepth)
		{
			this.controlMode = controlMode;
			this.returnVarName = returnVarName;
			this.flowControlDepth = flowControlDepth;
		}
	}
	
	private static string ConstructDepthCantExceedLimitError(int depth, int depthLimit)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Depth ({depth}) cannot exceed {depthLimit} for this mode!";
		}
		else
		{
			return $"Глубина ({depth}) не может превышать {depthLimit} для данного режима!";
		}
	}

	public enum ControlMode
	{
		StopProcess,
		ExitScope,
		StopCycle,
		SkipIteration
	}
}
