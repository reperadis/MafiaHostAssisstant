using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class NamedListStringConfigField : Control
{
	[Export] public Label label;
	[Export] private Control elementsContent;
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene elementScene;
	private List<string> list;
	private Action listener;

	public void SetUp(string label, List<string> current, Action listener)
	{
		this.label.Text = label;
		this.listener = listener;
		list = current;

		foreach (string item in current)
		{
			ListElement element = elementScene.Instantiate<ListElement>();
			element.SubscribeRemoval(RemoveElement);
			elementsContent.AddChild(element);
			StringConfigField field = boolFieldScene.Instantiate<StringConfigField>();
			field.SetUp(item, StringFieldContext.Unrestricted, true, v => ChangeElement(element.GetIndex(), v));
			element.fieldContent.AddChild(field);
		}
	}

	private void ChangeElement(int index, string v)
	{
		list[index] = v;
		listener?.Invoke();
	}

	public void AddElement() // Button
	{
		list.Add(string.Empty);
		ListElement element = elementScene.Instantiate<ListElement>();
		element.SubscribeRemoval(RemoveElement);
		elementsContent.AddChild(element);
		StringConfigField field = boolFieldScene.Instantiate<StringConfigField>();
		field.SetUp(string.Empty, StringFieldContext.Unrestricted, true, v => ChangeElement(element.GetIndex(), v));
		element.fieldContent.AddChild(field);
		listener?.Invoke();
	}

	private void RemoveElement(int index)
	{
		list.RemoveAt(index);
		listener?.Invoke();
	}
}
