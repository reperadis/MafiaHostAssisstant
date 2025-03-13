using Godot;

namespace MafiaHostAssistant;

public partial class BEListElement : Control
{
	[Export] private PackedScene boolFieldScene;
	[Export] private PackedScene intFieldScene;
	[Export] private PackedScene stringFieldScene;
	[Export] private PackedScene typeSearchFieldScene;

	private BEListConfigField listField;

	public void SetUp(BehaviorVariableType variableType, BEListConfigField listField, BEListElementData element, OperationScope refSearchScope, BehaviorEditor behaviorEditor)
	{
		this.listField = listField;

		if (element.isRef)
		{
			TypeSearchField field = boolFieldScene.Instantiate<TypeSearchField>();
			AddChild(field);
			field.SetUp(element.variable, refSearchScope, variableType, false, behaviorEditor, Redirect);
		}
		else
		{
			switch (variableType)
			{
				case BehaviorVariableType.Bool:
					{
						BoolConfigField field = boolFieldScene.Instantiate<BoolConfigField>();
						AddChild(field);
						field.SetUp((bool)element.directValue, b => Redirect(b));
					}
					break;
				case BehaviorVariableType.Integer:
					{
						IntConfigField field = boolFieldScene.Instantiate<IntConfigField>();
						AddChild(field);
						field.SetUp((int)element.directValue, true, i => Redirect(i));
					}
					break;
				case BehaviorVariableType.String:
					{
                        StringConfigField field = boolFieldScene.Instantiate<StringConfigField>();
						AddChild(field);
						field.SetUp((string)element.directValue, StringFieldContext.Unrestricted, true, s => Redirect(s));
					}
					break;
				default:
					break;
			}
		}
	}

	public void Remove()
	{
		listField.RemoveAt(GetIndex());
	}

	public void Redirect(object value)
	{

	}
}
