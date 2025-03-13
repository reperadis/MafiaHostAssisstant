using System.ComponentModel.Design.Serialization;
using Godot;

namespace MafiaHostAssistant;

public partial class HoldableButton : Button
{
	[Signal] public delegate void HeldEventHandler();
	[Export] private ProgressBar progressBar;
	private float holdDuration;
	private double progress;
	private bool isPressedDown;
	private bool awaitingUp;

	public override void _Ready()
	{		
		holdDuration = Settings.Instance.HoldDuration.Value;
		Settings.Instance.HoldDuration.Subscribe(this, OnDurationUpdated);
		progressBar.MaxValue = holdDuration;
	}

	public override void _Process(double delta)
	{
		if (!ButtonPressed || awaitingUp)
		{
			return;
		}
		progress += delta;
		UpdateProgressBar();
		if (progress >= holdDuration)
		{
			awaitingUp = true;
			EmitSignal(SignalName.Held);
		}
	}

    public void PointerDown()
	{
		isPressedDown = true;
	}

	private void PointerUp()
	{
		progress = 0;
		UpdateProgressBar();
		isPressedDown = false;
		awaitingUp = false;
	}

	private void OnVisibilityChanged()
	{
		progress = 0;
		UpdateProgressBar();
	}

	private void UpdateProgressBar()
	{
		progressBar.Value = progress;
	}

	private void OnDurationUpdated(float newDuration)
	{
		holdDuration = newDuration;
		progressBar.MaxValue = holdDuration;
	}
}
