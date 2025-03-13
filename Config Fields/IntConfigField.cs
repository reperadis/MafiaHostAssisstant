using Godot;
using System;

namespace MafiaHostAssistant;

public sealed partial class IntConfigField : Control
{
	[Export] private LineEdit lineEdit;

	private Action<int> reciever;
	private int value;
	private bool allowNegative;

	public void SetUp(int current, bool allowNegative, Action<int> reciever)
	{
		this.reciever = reciever;
		value = current;
		this.allowNegative = allowNegative;
		lineEdit.Text = value.ToString();
	}

	public void Increment()
	{
		if (value == 99_999_999) // A greater value won't fit int the LineEdit
		{
			return;
		}
		value++;
		lineEdit.Text = value.ToString();
		reciever?.Invoke(value);
	}

	public void Decrement()
	{
		if (!allowNegative && value == 0 || value == -9_999_999)
		{
			return;
		}
		value--;
		lineEdit.Text = value.ToString();
		reciever?.Invoke(value);
	}

	public void ReadInputField(string text)
	{
		if (int.TryParse(text, out value))
		{
			if (!allowNegative && value < 0)
			{
				lineEdit.Text = "0";
				value = 0;
			}
			reciever.Invoke(value);
		}
	}
}
