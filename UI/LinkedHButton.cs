using Godot;
using System;

namespace MafiaHostAssistant;

public partial class LinkedHButton : Button
{
	[Export] private ProgressBar progressBar;
	private double progress;
	private float holdThreshold;
	private bool isCurrent;
	private Action action;
	private DynamicStringDisplayer displayer;
	private int groupIndex;
	
	public void SetUp(DynamicStringDisplayer displayer, int groupIndex, float holdThreshold, Action action)
	{
		this.displayer = displayer;
		this.groupIndex = groupIndex;
		this.holdThreshold = holdThreshold;
		this.action = action;
		
		progressBar.MaxValue = holdThreshold;
	}

	public override void _Process(double delta)
	{
		if (!isCurrent)
		{
			return;
		}
		progress += delta;
		UpdateProgressBar();
		if (progress >= holdThreshold)
		{
			isCurrent = false;
			action?.Invoke();
		}
	}
	
	public void StartFilling()
	{
		isCurrent = true;
	}
	
	private void PointerUp()
	{
		displayer.StopAllButtonsFromGroup(groupIndex);
	}

	public void PointerUpManual()
	{
		progress = 0;
		UpdateProgressBar();
		isCurrent = false;
	}
	
	private void PointerDown()
	{
		displayer.StartFillingFirstInGroup(groupIndex);
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
}
