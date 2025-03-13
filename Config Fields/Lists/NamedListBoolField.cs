using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class NamedListBoolConfigField : Control
{
	[Export] public Label label;
	[Export] private Control elementsContent;
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene elementScene;
	private List<bool> list;
	private Action listener;

	public void SetUp(string label, List<bool> current, Action listener)
	{
		this.label.Text = label;
		this.listener = listener;
		list = current;

		foreach (bool item in current)
		{
			ListElement element = elementScene.Instantiate<ListElement>();
			element.SubscribeRemoval(RemoveElement);
			elementsContent.AddChild(element);
			BoolConfigField field = boolFieldScene.Instantiate<BoolConfigField>();
			field.SetUp(item, v => ChangeElement(element.GetIndex(), v));
			element.fieldContent.AddChild(field);
		}
	}

	private void ChangeElement(int index, bool v)
	{
		list[index] = v;
		listener?.Invoke();
	}

	public void AddElement() // Button
	{
		list.Add(false);
		ListElement element = elementScene.Instantiate<ListElement>();
		element.SubscribeRemoval(RemoveElement);
		elementsContent.AddChild(element);
		BoolConfigField field = boolFieldScene.Instantiate<BoolConfigField>();
		field.SetUp(false, v => ChangeElement(element.GetIndex(), v));
		element.fieldContent.AddChild(field);
		listener?.Invoke();
	}

	private void RemoveElement(int index)
	{
		list.RemoveAt(index);
		listener?.Invoke();
	}
}
