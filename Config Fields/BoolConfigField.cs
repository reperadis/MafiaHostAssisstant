using Godot;
using System;

namespace MafiaHostAssistant;

public sealed partial class BoolConfigField : Button
{
	private Action<bool> receiver;
	private bool value;

	public void SetUp(bool current, Action<bool> receiver)
	{
		this.receiver = receiver;
		value = current;
		Text = value ? Tr("TK:TRUE") : Tr("TK:FALSE");
	}

	public void SwitchAndRedirect()
	{
		value = !value;
		Text = value ? Tr("TK:TRUE") : Tr("TK:FALSE");
		receiver.Invoke(value);
	}
}
