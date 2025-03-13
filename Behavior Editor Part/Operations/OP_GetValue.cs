using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MafiaHostAssistant;

public partial class OP_GetValue : Operation
{
	[Export] private Label assignToVarNameLabel;
	[Export] private TextureRect assignToVarTypeTextureRect;
	[Export] private Label valueNameLabel;
	[Export] private TextureRect valueVarTypeTextureRect;
	[Export] private Label getFromVarNameLabel;
	[Export] private TextureRect getFromVarTypeTextureRect;

	private BehaviorEditor behaviorEditor;

	private BehaviorVariableHandler getFromVariableHandler;
	private BehaviorVariableHandler assignToVariableHandler;
	private int selectedOptionIndex;

	private Control optionEnumConfigFieldGO;
	private Control assignToConfigField;
	private readonly List<ValueOption> valueOptions = new();
	private BehaviorVariableType previousGetFromVarType = BehaviorVariableType.Nothing;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		getFromVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_READ-FROM"), BehaviorVariableType.Anything, getFromVarNameLabel, getFromVarTypeTextureRect);
		assignToVariableHandler = new(behaviorEditor, this, Tr("TK:OP_FIELD_WRITE-TO"), BehaviorVariableType.Nothing, assignToVarNameLabel, assignToVarTypeTextureRect);
		getFromVariableHandler.PostVariableRegistered += OnPostRegisterGetFromVariable;
	}

	public void OpenConfigWindow()
	{
		optionEnumConfigFieldGO = null;
		behaviorEditor.SetConfigWindowActive();
		getFromVariableHandler.CreateSelectionField(false);
		if (getFromVariableHandler.badVariableErrorIndex == -1)
		{
			optionEnumConfigFieldGO = behaviorEditor.CreateEnumField(Tr("TK:VALUE"), valueOptions.Select(t => new Dropdown.ElementData(Cache.Instance.GetVariableTypeTexture(t.valueType), t.valueDisplayName)).ToList(), selectedOptionIndex, RecieveSelectedOption);
		}
		if (valueOptions.Count != 0)
		{
			assignToVariableHandler.ChangeExpectedType(valueOptions[selectedOptionIndex].valueType);
			assignToConfigField = assignToVariableHandler.CreateSelectionField(true);
		}
	}

	private void OnPostRegisterGetFromVariable()
	{
		if (getFromVariableHandler.Variable.VariableType != previousGetFromVarType && optionEnumConfigFieldGO != null)
		{
			optionEnumConfigFieldGO.QueueFree();
			optionEnumConfigFieldGO = null;
			valueOptions.Clear();
			selectedOptionIndex = 0;
			previousGetFromVarType = getFromVariableHandler.Variable.VariableType;
		}

		if (getFromVariableHandler.Variable.VariableType == BehaviorVariableType.Player)
		{
			valueOptions.Add(new(Tr("TK:OP_FIELD_ROLE-NAME"), BehaviorVariableType.String, OptionName.RoleName)); // Role Name
			valueOptions.Add(new(Tr("TK:GET-VALUE_IS-ALIVE"), BehaviorVariableType.Bool, OptionName.IsAlive)); // Is Alive
		}
		else if (getFromVariableHandler.Variable.VariableType == BehaviorVariableType.Union)
		{
			valueOptions.Add(new(Tr("TK:GET-VALUE_PLAYERS"), BehaviorVariableType.ListOfPlayers, OptionName.Players)); // Players, including the ones in the nested Wakeables
		}
		else if (getFromVariableHandler.Variable.VariableType == BehaviorVariableType.Bool)
		{
			valueOptions.Add(new(Tr("TK:GET-VALUE_INVERSE-VAUE"), BehaviorVariableType.Bool, OptionName.InverseValue)); // Inverse Value
		}
		else if (getFromVariableHandler.Variable.VariableType == BehaviorVariableType.Integer)
		{
			valueOptions.Add(new(Tr("TK:GET-VALUE_INCREMENTED"), BehaviorVariableType.Integer, OptionName.Incremented)); // Incremented, which is "+= 1"
			valueOptions.Add(new(Tr("TK:GET-VALUE_DECREMENTED"), BehaviorVariableType.Integer, OptionName.Decremented)); // Decremented, which is "-= 1"
			valueOptions.Add(new(Tr("TK:GET-VALUE_ABSOLUTE"), BehaviorVariableType.Integer, OptionName.Absolute)); // Absolute Value
		}
		else
		{
			if (getFromVariableHandler.badVariableErrorIndex == -1)
			{ // TODO: Path is temporarily NULL
				getFromVariableHandler.badVariableErrorIndex = PushError(null, ConstructCannotTakeAnyValueFromVariableError(getFromVariableHandler.Variable), false);
			}
			return;
		}
		if (getFromVariableHandler.badVariableErrorIndex != -1)
		{
			ResolveError(getFromVariableHandler.badVariableErrorIndex);
			getFromVariableHandler.badVariableErrorIndex = -1;
		}
	}

	private void RecieveSelectedOption(int value)
	{
		selectedOptionIndex = value;
		ValueOption option = valueOptions[selectedOptionIndex];
		valueNameLabel.Text = option.valueDisplayName;
		valueVarTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(option.valueType);
		if (assignToConfigField != null && IsInstanceValid(assignToConfigField) && assignToVariableHandler.Variable.VariableType != option.valueType)
		{
			assignToConfigField.QueueFree();
			assignToVariableHandler.ChangeExpectedType(option.valueType);
			assignToVariableHandler.CreateSelectionField(true);
		}
	}

	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.GetValue, new Arguments(assignToVariableHandler.Variable.TrueVariableName, getFromVariableHandler.Variable.TrueVariableName, valueOptions[selectedOptionIndex].optionName, getFromVariableHandler.Variable.VariableType));
	}

	public override void Write(OperationReference.Arguments argumens)
	{
		Arguments args = (Arguments)argumens;
		getFromVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.getFromVarName));
		if (getFromVariableHandler.Variable == null) return;

		assignToVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.assignToVarName));
		if (assignToVariableHandler.Variable == null) return;

		selectedOptionIndex = valueOptions.FindIndex(t => t.optionName == args.selectedOptionName);
		if (selectedOptionIndex == -1)
		{
			selectedOptionIndex = 0;
		}
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_GET-VALUE");
	}

	public override bool IsStateless => true;

	[Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public string assignToVarName;
		public string getFromVarName;
		public BehaviorVariableType getFromVarType;
		public OptionName selectedOptionName;

		public Arguments(string assignToVarName, string getFromVarName, OptionName selectedOptionName, BehaviorVariableType getFromVarType)
		{
			this.assignToVarName = assignToVarName;
			this.getFromVarName = getFromVarName;
			this.selectedOptionName = selectedOptionName;
			this.getFromVarType = getFromVarType;
		}
	}

	public enum OptionName
	{
		RoleName,
		IsAlive,
		Players,
		InverseValue,
		Incremented,
		Decremented,
		Absolute
	}

	internal struct ValueOption
	{
		public string valueDisplayName;
		public BehaviorVariableType valueType;
		public OptionName optionName;

        public ValueOption(string valueDisplayName, BehaviorVariableType valueType, OptionName optionName)
        {
            this.valueDisplayName = valueDisplayName;
            this.valueType = valueType;
            this.optionName = optionName;
        }
    }

	private static string ConstructCannotTakeAnyValueFromVariableError(BehaviorVariable variable)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Cannot take any value from a varible ({variable.TranslatedVariableName}) of type {variable.VariableType.ToTranslatedFormatedStringLowercase()}!";
		}
		else
		{
			return $"Невозможно взять значение из переменной ({variable.TranslatedVariableName}) типа {variable.VariableType.ToTranslatedFormatedStringLowercase()}!";
		}
	}
}