using System;
using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class BEDynamicStringConfigField : Node
{
	[Export] private Label label;
	[Export] private Control elementsContent;
	[Export] private PackedScene dynamicStringScene;

	private List<BEDynamicStringElementData> message;
	private OperationScope searchScope;
	private BehaviorEditor behaviorEditor;

	public void SetUp(string label, OperationScope searchScope, List<BEDynamicStringElementData> operatedList, BehaviorEditor behaviorEditor)
	{
		this.label.Text = label;
		this.behaviorEditor = behaviorEditor;
		this.searchScope = searchScope;

		message = operatedList;
		message ??= new();

		if (message.Count != 0)
		{
			elementsContent.Visible = true;
		}

		foreach (BEDynamicStringElementData element in message)
		{
			BEDynamicStringElement elementObject = dynamicStringScene.Instantiate<BEDynamicStringElement>();
			elementsContent.AddChild(elementObject);
			elementObject.SetUp(this, element, searchScope, behaviorEditor);
		}
	}
	
	public void AddString()
	{
		BEDynamicStringElementData element = new(null, string.Empty, false);
		message.Add(element);
		BEDynamicStringElement elementObject = dynamicStringScene.Instantiate<BEDynamicStringElement>();
		elementsContent.AddChild(elementObject);
		elementObject.SetUp(this, element, searchScope, behaviorEditor);
		if (message.Count == 1)
		{
			elementsContent.Visible = true;
		}
	}
	
	public void AddVarRead()
	{
		BEDynamicStringElementData element = new(null, string.Empty, true);
		message.Add(element);
		BEDynamicStringElement elementObject = dynamicStringScene.Instantiate<BEDynamicStringElement>();
		elementsContent.AddChild(elementObject);
		elementObject.SetUp(this, element, searchScope, behaviorEditor);
		if (message.Count == 1)
		{
			elementsContent.Visible = true;
		}
	}
	
	public void NotifyRemove(int index)
	{
		message.RemoveAt(index);
		if (message.Count == 0)
		{
			elementsContent.Visible = false;
		}
	}
}

public class BEDynamicStringElementData
{
	public BehaviorVariable variable; // To track variables being deleted, renamed and retyped
	public string directString;
	public bool isVariable;

    public BEDynamicStringElementData(BehaviorVariable variable, string directString, bool isVariable)
    {
        this.variable = variable;
        this.directString = directString;
        this.isVariable = isVariable;
    }
}

[Serializable]
public class SharedDynamicStringElementData
{
	// Either the variable name (Stored and Runtime when inside scope execution),
	// or the converted data (variable values) (in Runtime when passed to a displayer)
	public bool isVariable;
	public string stringData;
	public BehaviorVariableType variableType;

    public SharedDynamicStringElementData(bool isVariable, string stringData, BehaviorVariableType variableType)
    {
        this.isVariable = isVariable;
        this.stringData = stringData;
        this.variableType = variableType;
    }
}

public interface IStringSource
{
	public string GetString();
	public event Action OnStringChanged;
}

public class DirectStringSource : IStringSource
{
	private readonly string directString;

	public DirectStringSource(string directString)
	{
		this.directString = directString;
	}

	// Direct string does not change
    public event Action OnStringChanged;

    public string GetString()
	{
		return directString;
	}
}

public class DisplayableDynamicStringElementData
{
	public bool isVariable;
	public BehaviorVariableType variableType;
	public readonly IStringSource StringSource;

    public DisplayableDynamicStringElementData(bool isVariable, BehaviorVariableType variableType, IStringSource stringSource)
    {
        this.isVariable = isVariable;
        this.variableType = variableType;
		StringSource = stringSource;
    }
}