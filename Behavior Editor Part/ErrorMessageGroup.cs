using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public sealed partial class ErrorMessageGroup : Node
{
	[Export] private Label operationNameLabel;
	[Export] private Node content;
	[Export] private PackedScene errorMessageScene;
	private Operation operation;
	private string[] docsPath;
	private BEDocumentationWindow docsWindow;
	private readonly Dictionary<int, bool> errorBlockTable = new();

	public void SetOperation(BEDocumentationWindow docsWindow, string[] docsPath, Operation operation)
	{
		this.docsWindow = docsWindow;
		this.docsPath = docsPath;
		this.operation = operation;
		operationNameLabel.Text = operation.GetReadableOpearationName();
	}

	public int AddError(string messageText, bool isSaveBlocking)
	{
		ColorEmbedLabel message = errorMessageScene.Instantiate<ColorEmbedLabel>();
		message.Text = messageText;
		content.AddChild(message);
		int index = content.GetChildCount() - 1;
		errorBlockTable.TryAdd(index, isSaveBlocking);
		return index;
	}

	public bool RemoveErrorAndDestroyIfFullyResolved(int index)
	{
		// -2 to account for one child being removed and another one being the Header
		if (content.GetChildCount() - 2 == 0)
		{
			QueueFree();
			return true;
		}
		errorBlockTable.Remove(index);
		content.GetChild(index).QueueFree();
		return false;
	}

	public void ResolveAllAndDestroy()
	{
		QueueFree();
	}
	
	public void GoToOperation()
	{
		// TODO: Open the beh. window, scroll to the operation
	}

	public void GoToErrorDocs()
	{
		docsWindow.Visible = true;
		docsWindow.GoToDocumentationPart(docsPath);
	}

	public bool IsSaveBlocking()
	{
		foreach (bool isBlocking in errorBlockTable.Values)
		{
			if (isBlocking)
			{
				return true;
			}
		}
		return false;
	}
}
