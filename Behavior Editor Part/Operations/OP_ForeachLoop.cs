using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class OP_ForeachLoop : Operation
{
	[Export] private OperationScope behaviorScope;
	[Export] private Label listVarNameLabel;
	[Export] private TextureRect listVarTypeTextureRect;
	[Export] private Label elementNameLabel;
	[Export] private TextureRect elementTypeTextureRect;
	
	private static readonly string[] ErrorsPath = { "Foreach loop", "Errors" };

	private BehaviorEditor behaviorEditor;
	private BehaviorVariableHandler listVariableHandler;
	private readonly BehaviorVariable elementVariable = new(string.Empty, BehaviorVariableType.Nothing, true);
	private int badElementNameErrorIndex;

	protected override void OnAddition(BehaviorEditor behaviorEditor)
	{
		this.behaviorEditor = behaviorEditor;
		ParentScope.OnVariableAddedOrRenamed += OnVariableAdded;
		behaviorScope.SetUp(ParentScope, this, ParentScope.MustExitWithBool, ParentScope.RootEntryPoint, behaviorEditor);
		listVariableHandler = new(behaviorEditor, this, Tr("TK:VARTYPE_LIST"), BehaviorVariableType.ListOfAnything, listVarNameLabel, listVarTypeTextureRect);
		listVariableHandler.PostVariableTypeChanged += OnPostVariableTypeChanged;
		listVariableHandler.PostVariableRegistered += OnPostIteratedVariableRegistered;
		badElementNameErrorIndex = PushError(ErrorsPath, ConstructElementMustHaveNameError(), false);
	}

	public void OpenConfigWindow()
	{
		behaviorEditor.SetConfigWindowActive();
		listVariableHandler.CreateSelectionField(true);
		behaviorEditor.CreateStringField("TK:OP_FIELD_ELEMENT-NAME", elementVariable.TrueVariableName, RecieveIteratedItemName);
	}

	private void OnPostVariableTypeChanged()
	{
		if (!listVariableHandler.Variable.VariableType.IsList())
		{
			behaviorScope.variables.Remove(elementVariable);
			elementVariable.InvokeRemoval();
			return; // Error is already pushed by the handler
		}
		elementVariable.VariableType = GetContainedListType(listVariableHandler.Variable.VariableType);
	}

	private void OnVariableAdded(BehaviorVariable addedVariable)
	{
		if (addedVariable.TrueVariableName == elementVariable.TrueVariableName)
		{
			if (badElementNameErrorIndex != -1)
			{
				ResolveError(badElementNameErrorIndex);
			}
			badElementNameErrorIndex = PushError(ErrorsPath, ConstructElementNameConflictsWithHigherPriorityError(elementVariable.TranslatedVariableName), true);
		}
	}

	private void OnPostIteratedVariableRegistered()
	{
		if (listVariableHandler.Variable == null)
		{
			behaviorScope.variables.Remove(elementVariable);
			elementVariable.InvokeRemoval();
			return;
		}

		BehaviorVariableType containedType = GetContainedListType(listVariableHandler.Variable.VariableType);
		elementVariable.isPersisting = listVariableHandler.Variable.isPersisting;
		if (elementVariable.VariableType != containedType)
		{
			elementVariable.VariableType = containedType; // This invokes OnTypeChanged
			elementTypeTextureRect.Texture = Cache.Instance.GetVariableTypeTexture(elementVariable.VariableType);
		}

		if (badElementNameErrorIndex == -1 && !behaviorScope.variables.Contains(elementVariable))
		{
			behaviorScope.variables.Add(elementVariable);
			behaviorScope.OnVariableAddedOrRenamed?.Invoke(elementVariable);
		}
	}

	private void RecieveIteratedItemName(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			if (badElementNameErrorIndex != -1)
			{
				ResolveError(badElementNameErrorIndex);
			}
			badElementNameErrorIndex = PushError(ErrorsPath, ConstructElementMustHaveNameError(), false);
			elementVariable.TrueVariableName = "";
			elementNameLabel.Text = "@Null";
			elementTypeTextureRect.Texture= Cache.Instance.GetVariableTypeTexture(BehaviorVariableType.Nothing);
			return;
		}
		elementVariable.TrueVariableName = value;
		elementNameLabel.Text = value;
		if (ParentScope.FindVariableByName(value) != null)
		{
			if (badElementNameErrorIndex != -1)
			{
				ResolveError(badElementNameErrorIndex);
			}
			badElementNameErrorIndex = PushError(ErrorsPath, ConstructVariableNameAlreadyExistsError(value), true);
			return;
		}

		if (badElementNameErrorIndex != -1)
		{
			ResolveError(badElementNameErrorIndex);
			badElementNameErrorIndex = -1;
		}

		if (listVariableHandler.badVariableErrorIndex == -1 && !behaviorScope.variables.Contains(elementVariable))
		{
			behaviorScope.variables.Add(elementVariable);
		}
		behaviorScope.OnVariableAddedOrRenamed?.Invoke(elementVariable);
	}

	protected override void OnDeletion()
	{
		foreach (Node node in behaviorScope.GetChildren())
		{
			if (node is Operation operation)
			{
				operation.Delete();
			}
		}
		ParentScope.OnVariableAddedOrRenamed -= OnVariableAdded;
	}

	private static BehaviorVariableType GetContainedListType(BehaviorVariableType listType)
	{
		return listType switch
		{
			BehaviorVariableType.ListOfBools => BehaviorVariableType.Bool,
			BehaviorVariableType.ListOfInts => BehaviorVariableType.Integer,
			BehaviorVariableType.ListOfStrings => BehaviorVariableType.String,
			BehaviorVariableType.ListOfPlayers => BehaviorVariableType.Player,
			_ => throw new Exception("Cannot get the contained list type for BehaviorVariableType " + listType)
		};
	}
	
	public override OperationReference GetOperationReference()
	{
		return new OperationReference(OperationName.ForeachLoop, new Arguments(listVariableHandler.Variable.TrueVariableName, elementVariable.TrueVariableName, behaviorScope.ReadScope()));
	}
	
	public override void Write(OperationReference.Arguments arguments)
	{
		Arguments args = (Arguments)arguments;
		listVariableHandler.RegisterVariable(ParentScope.FindVariableByName(args.iterateOnVarName));

		elementVariable.TrueVariableName = args.iterateAsVarName;
		elementVariable.VariableType = GetContainedListType(listVariableHandler.Variable.VariableType);
		elementNameLabel.Text = args.iterateAsVarName;
		elementTypeTextureRect.Texture= Cache.Instance.GetVariableTypeTexture(elementVariable.VariableType);
		behaviorScope.variables.Add(elementVariable);

		behaviorScope.WriteScope(args.sequence);
	}

	public override string GetReadableOpearationName()
	{
		return Tr("TK:OP_FOREACH-LOOP");
	}

	public override bool IsStateless => behaviorScope.IsStateless(); // TODO: I believe only Ini scope can be not stateless, because only it can host special variable creators

	[Serializable]
	public class Arguments : OperationReference.Arguments
	{
		public string iterateOnVarName;
		public string iterateAsVarName;
		public List<OperationReference> sequence;

		public Arguments(string iterateOnVarName, string iterateAsVarName, List<OperationReference> sequence)
		{
			this.iterateOnVarName = iterateOnVarName;
			this.iterateAsVarName = iterateAsVarName;
			this.sequence = sequence;
		}
	}

	private static string ConstructElementMustHaveNameError()
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return "Element must have a name!";
		}
		else
		{
			return "Элемент должен иметь имя!";
		}
	}
	
	private static string ConstructElementNameConflictsWithHigherPriorityError(string elementName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Element name ({elementName}) conflicts with the name of a variable of a higher priority!";
		}
		else
		{
			return $"Имя элемента ({elementName}) конфликтует с именем переменной более высокого приоритета!";
		}
	}

	protected static string ConstructVariableNameAlreadyExistsError(string variableName)
	{
		if (TranslationServer.GetLocale() == "en")
		{
			return $"Variable with the name \"{variableName}\" already exists!";
		}
		else
		{
			return $"Переменная с именем \"{variableName}\" уже существует!";
		}
	}
}
