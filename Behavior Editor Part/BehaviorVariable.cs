using Godot;
using System;

namespace MafiaHostAssistant;

public class BehaviorVariable : IStringSource
{
	private string variableName;
	private BehaviorVariableType variableType;
	public string TrueVariableName
	{
		get => variableName;
		set
		{
			if (variableName == value)
			{
				return;
			}
			variableName = value;
			OnVariableRenamed?.Invoke();
		}
	}

	public string TranslatedVariableName => Translator.TryTranslateBehaviorVariableName(variableName);
	
	public BehaviorVariableType VariableType
	{
		get => variableType;
		set
		{
			if (variableType == value)
			{
				return;
			}
			variableType = value;
			OnVariableTypeChanged?.Invoke();
		}
	}
	
	public bool IsInvalid = true;

	public bool IsReadOnly { get; private set; }
	public bool isPersisting;

	public BehaviorVariable(string variableName, BehaviorVariableType variableType, bool isReadOnly)
	{
		this.variableName = variableName;
		this.variableType = variableType;
		IsReadOnly = isReadOnly;
	}

    public void InvokeRemoval()
	{
		OnVariableRemoved?.Invoke();
	}

	public VariableRecord Deconstruct()
	{
		return new(variableName, variableType);
	}

	public event Action OnVariableRenamed;
	public event Action OnVariableTypeChanged;
	public event Action OnVariableRemoved;

	public bool IsList()
	{
		return variableType switch
		{
			BehaviorVariableType.ListOfBools or BehaviorVariableType.ListOfInts or BehaviorVariableType.ListOfStrings or BehaviorVariableType.ListOfPlayers => true,
			_ => false,
		};
	}

	event Action IStringSource.OnStringChanged
	{
		add
		{
			OnVariableRenamed += value;
		}

		remove
		{
			OnVariableRenamed += value;
		}
	}

	string IStringSource.GetString()
    {
        return variableName;
    }

}

// TODO: Subscribe to Settings' ThemeChanged and refresh the sprites on varTypeTextureRects.
// KEEP THIS AS A NOTE AFTER IMPLEMENTING:
// BehaviorEditor subscribes to ThemeChanged before any of the handlers and always refreshes the type textures cache before the handlers fetch them

public class BehaviorVariableHandler
{
	private static readonly string[] FieldIsEmptyErrorPath = { "" }; // TODO: Create the paths
	private static readonly string[] IncompatibleNewTypeErrorPath = { "" };
	private static readonly string[] VariableDeletedErrorPath = { "" };

	public BehaviorVariable Variable { get; private set; }
	private readonly Label varNameLabel;
	private readonly TextureRect varTypeTextureRect;
	
	private readonly BehaviorEditor behaviorEditor;
	private readonly Operation hostOperation;
	private readonly string fieldName;
	private BehaviorVariableType expectedVariableType;

	public int badVariableErrorIndex = -1;
	public event Action PostVariableRegistered;
	public event Action PostVariableTypeChanged;
	public event Action PostVariableRemoved;

	public BehaviorVariableHandler(BehaviorEditor behaviorEditor, Operation hostOperation, string fieldName, BehaviorVariableType expectedVariableType, Label varNameLabel, TextureRect varTypeTextureRect)
	{
		this.behaviorEditor = behaviorEditor;
		this.hostOperation = hostOperation;
		this.fieldName = fieldName;
		this.expectedVariableType = expectedVariableType;
		this.varNameLabel = varNameLabel;
		this.varTypeTextureRect = varTypeTextureRect;

		Settings.Instance.IsLightTheme.Subscribe(hostOperation, OnThemeChanged);

		RegisterVariable(BehaviorEditor.NullVariable);
	}

	public NamedTypeSearchField CreateSelectionField(bool excludeSystemVariables)
	{
		return behaviorEditor.CreateTypeSearchField(fieldName, hostOperation.ParentScope, expectedVariableType, excludeSystemVariables, Variable, RegisterVariable);
	}

	private void OnVariableRenamed()
	{
		varNameLabel.Text = Variable.TranslatedVariableName;
	}

	private void OnVariableTypeChanged()
	{
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(Variable.VariableType);
		if (badVariableErrorIndex != -1)
		{
			hostOperation.ResolveError(badVariableErrorIndex);
		}

		bool shouldPushError = false;
		if (expectedVariableType != BehaviorVariableType.Anything)
		{
			if (expectedVariableType == BehaviorVariableType.ListOfAnything)
			{
				shouldPushError = !Variable.VariableType.IsList();
			}
			else
			{
				shouldPushError = Variable.VariableType != expectedVariableType;
			}
		}
		if (shouldPushError)
		{
			badVariableErrorIndex = hostOperation.PushError(IncompatibleNewTypeErrorPath, Operation.ConstructOperationIncompatibleWithNewTypeError(Variable.TranslatedVariableName), false);
			SetVarNameLabelRed();
		}
		else
		{
			badVariableErrorIndex = -1;
			SetVarNameLabelNormal();
		}
		PostVariableTypeChanged?.Invoke();
	}

	private void OnVariableRemoved()
	{
		varNameLabel.Text = BehaviorEditor.NullVariable.TranslatedVariableName;
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Nothing);
		if (badVariableErrorIndex != -1)
		{
			hostOperation.ResolveError(badVariableErrorIndex);
		}
		badVariableErrorIndex = hostOperation.PushError(VariableDeletedErrorPath, Operation.ConstructVariableDeletedError(Variable.TranslatedVariableName), false);
		SetVarNameLabelRed();
		UnregisterVariableEvents();
		Variable = BehaviorEditor.NullVariable;
		PostVariableRemoved?.Invoke();
	}

	public void RegisterVariable(BehaviorVariable newVariable)
	{
		if (Variable == newVariable)
		{
			return;
		}
		UnregisterVariableEvents();
		Variable = newVariable;
		if (Variable == BehaviorEditor.NullVariable)
		{
			if (badVariableErrorIndex != -1)
			{
				hostOperation.ResolveError(badVariableErrorIndex);
			}
			varNameLabel.Text = Variable.TranslatedVariableName;
			varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Nothing);
			if (expectedVariableType != BehaviorVariableType.Nothing)
			{
				badVariableErrorIndex = hostOperation.PushError(FieldIsEmptyErrorPath, Operation.ConstructFieldIsEmptyError(fieldName), false);
				SetVarNameLabelRed();
			}
			return;
		}
		
		bool shouldPushError = false;
		if (expectedVariableType != BehaviorVariableType.Anything)
		{
			if (expectedVariableType == BehaviorVariableType.ListOfAnything)
			{
				shouldPushError = !Variable.VariableType.IsList();
			}
			else
			{
				shouldPushError = Variable.VariableType != expectedVariableType;
			}
		}
		if (shouldPushError)
		{
			badVariableErrorIndex = hostOperation.PushError(IncompatibleNewTypeErrorPath, Operation.ConstructOperationIncompatibleWithNewTypeError(Variable.TranslatedVariableName), false);
			SetVarNameLabelRed();
		}
		
		RegisterVariableEvents();
		if (badVariableErrorIndex != -1)
		{
			hostOperation.ResolveError(badVariableErrorIndex);
			badVariableErrorIndex = -1;
			SetVarNameLabelNormal();
		}
		varNameLabel.Text = Variable.TranslatedVariableName;
		varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(newVariable.VariableType);
		PostVariableRegistered?.Invoke();
	}

	public void ChangeExpectedType(BehaviorVariableType newType)
	{
		expectedVariableType = newType;

		if (Variable == BehaviorEditor.NullVariable)
		{
			if (expectedVariableType != BehaviorVariableType.Nothing)
			{
				badVariableErrorIndex = hostOperation.PushError(FieldIsEmptyErrorPath, Operation.ConstructFieldIsEmptyError(fieldName), false);
				SetVarNameLabelRed();
			}
			return;
		}
		
		bool shouldPushError = false;
		if (expectedVariableType != BehaviorVariableType.Anything)
		{
			if (expectedVariableType == BehaviorVariableType.ListOfAnything)
			{
				shouldPushError = !Variable.VariableType.IsList();
			}
			else
			{
				shouldPushError = Variable.VariableType != expectedVariableType;
			}
		}
		if (shouldPushError)
		{
			badVariableErrorIndex = hostOperation.PushError(IncompatibleNewTypeErrorPath, Operation.ConstructOperationIncompatibleWithNewTypeError(Variable.TranslatedVariableName), false);
			SetVarNameLabelRed();
		}
		else
		{
			badVariableErrorIndex = -1;
			SetVarNameLabelNormal();
		}
	}

	private void RegisterVariableEvents()
	{
		Variable.OnVariableRenamed += OnVariableRenamed;
		Variable.OnVariableTypeChanged += OnVariableTypeChanged;
		Variable.OnVariableRemoved += OnVariableRemoved;
	}

	private void UnregisterVariableEvents()
	{
		// Comparing with null is intended
		if (Variable != null && Variable != BehaviorEditor.NullVariable)
		{
			Variable.OnVariableRenamed -= OnVariableRenamed;
			Variable.OnVariableTypeChanged -= OnVariableTypeChanged;
			Variable.OnVariableRemoved -= OnVariableRemoved;
		}
	}
	
	private void OnThemeChanged(bool _)
	{
		if (Variable == null)
		{
			varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Nothing);
		}
		else
		{
			varTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(Variable.VariableType);
		}
	}

	private void OnHostDeleting()
	{
		hostOperation.TreeExiting -= OnHostDeleting; // To not prevent Garbage Collection
		UnregisterVariableEvents();
	}
	
	private void SetVarNameLabelRed()
	{
		if (!varNameLabel.HasThemeColorOverride("font_color"))
		{
			varNameLabel.AddThemeColorOverride("font_color", Cache.Instance.ErroringRed);
		}
	}
	
	private void SetVarNameLabelNormal()
	{
		if (varNameLabel.HasThemeColorOverride("font_color"))
		{
			varNameLabel.RemoveThemeColorOverride("font_color");
		}
	}
}