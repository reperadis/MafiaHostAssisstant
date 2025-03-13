using Godot;

namespace MafiaHostAssistant;

public sealed partial class BEDynamicStringElement : Control
{
	[Export] private Control fieldContent;
	[Export] private PackedScene stringFieldScene;
	[Export] private PackedScene typeSearchFieldScene;

	private BEDynamicStringConfigField configField;
	
	public void SetUp(BEDynamicStringConfigField configField, BEDynamicStringElementData element, OperationScope refSearchScope, BehaviorEditor behaviorEditor)
	{
		this.configField = configField;
		if (element.isVariable)
		{
			TypeSearchField field = typeSearchFieldScene.Instantiate<TypeSearchField>();
			field.SetUp(element.variable, refSearchScope, BehaviorVariableType.Anything, false, behaviorEditor, v => element.variable = v);
			fieldContent.AddChild(field);
			fieldContent.MoveChild(field, 0);
		}
		else
		{
			StringConfigField field = stringFieldScene.Instantiate<StringConfigField>();
			field.SetUp(element.directString, StringFieldContext.Unrestricted, true, s => element.directString = s);
			fieldContent.AddChild(field);
			fieldContent.MoveChild(field, 0);
		}
	}
	public void Remove()
	{
		configField.NotifyRemove(GetIndex());
		QueueFree();
	}
}
