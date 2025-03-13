using Godot;
using System;
using System.IO;

namespace MafiaHostAssistant;

public partial class StringConfigField : TextEdit
{
	private Action<string> receiver;
	private bool isProcessing;

	public void SetUp(string current, StringFieldContext context, bool allowMultiline, Action<string> receiver)
	{
		this.receiver = receiver;
		if (!string.IsNullOrEmpty(current))
		{
			Text = current;
		}
		else
		{
			Text = string.Empty;
		}

		if (context == StringFieldContext.BehaviorVariableName)
		{
			TextChanged += OnEditBehaviorVariable;
			TextSet += OnEditBehaviorVariable;
			FocusExited += OnEditBehaviorVariable;
		}
		else if (context == StringFieldContext.FileName)
		{
			TextChanged += OnEditFileName;
			TextSet += OnEditFileName;
			FocusExited += OnEditFileName;
		}
		
		if (!allowMultiline)
		{
			TextChanged += OnEditSingleLine;
			TextSet += OnEditSingleLine;
			FocusExited += OnEditSingleLine;
		}
		FocusExited += Redirect;
	}
	
	private void OnEditSingleLine()
	{
		if (isProcessing || string.IsNullOrEmpty(Text)) return;
		isProcessing = true;
		int line = GetCaretLine(); // Setting the Text resets caret position to the start of the text;
		int column = GetCaretColumn();
		Text = Text.Replace("\n", string.Empty);
		SetCaretLine(line);
		SetCaretColumn(column);
		isProcessing = false;
	}

	private void OnEditBehaviorVariable()
	{
		if (isProcessing || string.IsNullOrEmpty(Text)) return;
		isProcessing = true;
		int line = GetCaretLine();
		int column = GetCaretColumn();
		Text = Text.Replace("@", string.Empty);
		SetCaretLine(line);
		SetCaretColumn(column);
		isProcessing = false;
	}

	private void OnEditFileName()
	{
		if (isProcessing || string.IsNullOrEmpty(Text)) return;
		isProcessing = true;
		int line = GetCaretLine();
		int column = GetCaretColumn(); // TODO: Use Path.GetInvalidFileNameChars()
		Text = Text.Replace("/", string.Empty).Replace("\\", string.Empty).Replace(":", string.Empty).Replace("*", string.Empty).Replace("?", string.Empty).Replace("\"", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty).Replace("|", string.Empty).Replace("@", string.Empty);
		SetCaretLine(line);
		SetCaretColumn(column);
		isProcessing = false;
	}

	private void Redirect()
	{
		receiver?.Invoke(Text);
	}
}

public enum StringFieldContext
{
	BehaviorVariableName,
	FileName,
	Unrestricted
}