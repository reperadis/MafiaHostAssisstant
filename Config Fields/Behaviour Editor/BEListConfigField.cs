using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;
// TODO: Allow Lists to be included into lists; i.e. ListOfInts can be formed as {ListOfInts {1, 2}, 3, 4}, resulting in ListOfInts {1, 2, 3, 4}
public sealed partial class BEListConfigField : Control
{
	[Export] private Label label;
	[Export] private PackedScene listElementScene;
	[Export] private Control content;
	[Export] private Control addValButton;

	private BehaviorVariableType containedVarType;
	private List<BEListElementData> values;
	private OperationScope refCodeScope;
	private BehaviorEditor behaviorEditor;

	public void SetUp(string label, List<BEListElementData> operatedList, BehaviorVariableType containedVarType, OperationScope refCodeScope, BehaviorEditor behaviorEditor)
	{
		this.label.Text = label;
		this.behaviorEditor = behaviorEditor;
		values = operatedList;
		if (values.Count > 0)
		{
			content.Visible = true;
		}
		this.containedVarType = containedVarType;
		this.refCodeScope = refCodeScope;
		if (containedVarType == BehaviorVariableType.Player)
		{
			addValButton.Visible = false;
		}
		foreach (BEListElementData item in values)
		{
			BEListElement lElement = listElementScene.Instantiate<BEListElement>();
			content.AddChild(lElement);
			lElement.SetUp(containedVarType, this, item, refCodeScope, behaviorEditor);
		}
	}

	public void RemoveAt(int index)
	{
		values.RemoveAt(index);
		GetChild(index).QueueFree();
		if (values.Count == 0)
		{
			content.Visible = false;
		}
	}

	public void AddValElement()
	{
		BEListElementData element = new() { isRef = false, directValue = GetDefault() };
		values.Add(element);
		if (values.Count == 1)
		{
			content.Visible = true;
		}
		BEListElement lElement = listElementScene.Instantiate<BEListElement>();
		content.AddChild(lElement);
		lElement.SetUp(containedVarType, this, element, refCodeScope, behaviorEditor);
	}
	
	public void AddRefElement()
	{
		BEListElementData element = new() { isRef = true, variable = null };
		values.Add(element);
		if (values.Count == 1)
		{
			content.Visible = true;
		}
		BEListElement lElement = listElementScene.Instantiate<BEListElement>();
		content.AddChild(lElement);
		lElement.SetUp(containedVarType, this, element, refCodeScope, behaviorEditor);
	}
	private object GetDefault()
	{
		return containedVarType switch
		{
			BehaviorVariableType.Bool => false,
			BehaviorVariableType.Integer => 0,
			BehaviorVariableType.String => string.Empty,
			BehaviorVariableType.Player => string.Empty,// Name of variable containing the player
			_ => null,
		};
	}
}

public class BEListElementData
{
	public object directValue;
	public bool isRef;
	public BehaviorVariable variable;
}
