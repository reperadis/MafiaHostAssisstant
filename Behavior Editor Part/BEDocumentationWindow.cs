using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class BEDocumentationWindow : Control
{
	[Export] private ScrollContainer scrollContainer;
	private readonly Dictionary<string, BEDocumentationElement> topLevelElements = new();

	public void SetUp()
	{
		foreach (Node child in scrollContainer.GetChild(0).GetChildren())
		{
			// TODO: Each node is Documentation Element that defines the Y position relative to their root content to allow setting scrollContainer's scroll to it.
		}
	}

	public void GoToDocumentationPart(string[] path)
	{
		
	}
}
