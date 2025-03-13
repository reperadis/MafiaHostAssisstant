using Godot;
using System;

namespace MafiaHostAssistant;

public partial class ListElement : Control
{
	[Export] public Control fieldContent;
	private Action<int> removalReceiver;

	public void SubscribeRemoval(Action<int> removalReceiver)
	{
		this.removalReceiver = removalReceiver;
	}

	public void Remove()
	{
		removalReceiver.Invoke(GetIndex());
		QueueFree();
	}
}
