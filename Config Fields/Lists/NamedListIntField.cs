using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class NamedListIntConfigField : Control
{
	[Export] public Label label;
	[Export] private Control elementsContent;
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene elementScene;
	private List<int> list;
	private Action listener;

	public void SetUp(string label, List<int> current, Action listener)
	{
		this.label.Text = label;
		this.listener = listener;
		list = current;

		foreach (int item in current)
		{
			ListElement element = elementScene.Instantiate<ListElement>();
			element.SubscribeRemoval(RemoveElement);
			elementsContent.AddChild(element);
			IntConfigField field = boolFieldScene.Instantiate<IntConfigField>();
			field.SetUp(item, true, v => ChangeElement(element.GetIndex(), v));
			element.fieldContent.AddChild(field);
		}
	}

	private void ChangeElement(int index, int v)
	{
		list[index] = v;
		listener?.Invoke();
	}

	public void AddElement() // Button
	{
		list.Add(0);
		ListElement element = elementScene.Instantiate<ListElement>();
		element.SubscribeRemoval(RemoveElement);
		elementsContent.AddChild(element);
		IntConfigField field = boolFieldScene.Instantiate<IntConfigField>();
		field.SetUp(0, true, v => ChangeElement(element.GetIndex(), v));
		element.fieldContent.AddChild(field);
		listener?.Invoke();
	}

	private void RemoveElement(int index)
	{
		list.RemoveAt(index);
		listener?.Invoke();
	}
}
