using Godot;

namespace MafiaHostAssistant;

public sealed partial class OperationPad : Control
{
	[Export] private PackedScene operation;
	
	public override Variant _GetDragData(Vector2 atPosition)
	{
		SetDragPreview(operation.Instantiate<Control>());
		return operation;
	}
}
