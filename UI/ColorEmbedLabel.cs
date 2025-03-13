using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

[Tool]
public partial class ColorEmbedLabel : CustomLabel
{
	public class TagRectInfo
	{
		public string tagName;
		public Rect2 rect;
		public int start;
		public int end;
	}

	private StyleBoxFlat embedStyle;

	private readonly List<Panel> panels = new();
	private readonly List<TagRectInfo> embedTagRects = new();

	private readonly List<TagRange> embedRanges = new();
	private readonly List<TagRange> typeTagRanges = new();
	private string displayText;

    public override void _Ready()
    {
		bool isLightTheme;

		if (Engine.IsEditorHint())
		{
			isLightTheme = true;
		}
		else
		{
			isLightTheme = Settings.Instance.IsLightTheme.Value;
			Settings.Instance.IsLightTheme.Subscribe(this, UpdateEmbedStyle);
		}

		embedStyle = new StyleBoxFlat()
		{
			BgColor = new Color(isLightTheme ? "F0F0F0" : "2c3e50"), // TODO: Find the best looking color
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8,
			CornerRadiusBottomLeft = 8,
			CornerRadiusBottomRight = 8
		};

		base._Ready();
	}

    private void UpdateEmbedStyle(bool newIsLightTheme)
	{
		embedStyle.BgColor = new Color(newIsLightTheme ? "F0F0F0" : "2c3e50");
	}

    protected override string OnUpdateParagraph()
	{
		embedTagRects.Clear();
		embedRanges.Clear();
		typeTagRanges.Clear();
		displayText = Tr(Text);
		while (true)
		{
			int openTagStart = displayText.IndexOf("[e]");
			if (openTagStart == -1) break; // No more tags

			int closingTagStart = displayText.IndexOf("[/e]");
			if (closingTagStart == -1)
			{
				if (!Engine.IsEditorHint()) GD.PrintErr($"Tag is not closed in text '{Text}'");
				return null; // Tag is not closed
			}

			int nestedEmbedStart = displayText.IndexOf("[e]", openTagStart + 3);
			if (nestedEmbedStart != -1 && nestedEmbedStart < closingTagStart)
			{
				if (!Engine.IsEditorHint()) GD.PrintErr($"Nested embeds found in text '{Text}'");
				return null; // Nested embeds are invalid
			}

			embedRanges.Add(new TagRange
			{
				// Both are adjusted for removal of tags and newlines
				// If openTagStart is zero, CountN will count all newlines instead of none
				contentStart = openTagStart == 0? 0 : openTagStart - displayText.CountN("\n", 0, openTagStart),
				contentEnd = closingTagStart - 3 - displayText.CountN("\n", 0, closingTagStart)
			});
			displayText = displayText.Remove(openTagStart, 3);
			displayText = displayText.Remove(closingTagStart - 3, 4);
		}

		string lastOpenTagName = null;
		int lastOpenTagStart = -1;
		int lastOpenTagEnd = -1;
		while (true)
		{
			int tagStart = displayText.IndexOf("[", lastOpenTagEnd == -1 ? 0 : lastOpenTagEnd + 1);
			if (tagStart == -1) break; // No more tags

			int tagEnd = displayText.IndexOf("]", lastOpenTagEnd == -1 ? 0 : lastOpenTagEnd + 1);
			if (tagEnd == -1)
			{
				if (!Engine.IsEditorHint()) GD.PrintErr($"Tag is not closed in text '{Text}'");
				return null;
			}

			string tag = displayText[(tagStart + 1)..tagEnd];

			if (tag.StartsWith('/'))
			{
				tag = tag[1..]; // Remove the '/'
				if (lastOpenTagName == null)
				{
					if (!Engine.IsEditorHint()) GD.PrintErr($"Closing tag '{tag}' without an opening tag in text '{Text}'");
					return null;
				}

				if (lastOpenTagName != tag)
				{
					if (!Engine.IsEditorHint()) GD.PrintErr($"Closing tag '{tag}' does not match the opening tag '{lastOpenTagName}' in text '{Text}'");
					return null;
				}

				int contentStart = lastOpenTagEnd - lastOpenTagName.Length - 1;
				int contentEnd = tagStart - lastOpenTagName.Length - 3;

				typeTagRanges.Add(new TagRange
				{
					contentStart = contentStart - displayText.Count("\n", 0, contentStart),
					contentEnd = contentEnd - displayText.Count("\n", 0, tagStart),
					tagName = tag
				});

				displayText = displayText.Remove(lastOpenTagStart, lastOpenTagName.Length + 2);
				displayText = displayText.Remove(tagStart - (lastOpenTagName.Length + 2), tag.Length + 3);

				foreach (TagRange range in embedRanges)
				{
					int adjustedStart = range.contentStart;
					int adjustedEnd = range.contentEnd;
					if (range.contentStart > contentStart)
					{
						adjustedStart -= lastOpenTagName.Length + 2;
					}
					if (range.contentEnd > contentStart)
					{
						adjustedEnd -= lastOpenTagName.Length + 2;
					}
					if (range.contentStart > contentEnd)
					{
						adjustedStart -= tag.Length + 3;
					}
					if (range.contentEnd > contentEnd)
					{
						adjustedEnd -= tag.Length + 3;
					}
					range.contentStart = adjustedStart;
					range.contentEnd = adjustedEnd;
				}
				lastOpenTagName = null;
				lastOpenTagStart = -1;
				lastOpenTagEnd = -1;
			}
			else
			{
				lastOpenTagName = tag;
				lastOpenTagStart = tagStart;
				lastOpenTagEnd = tagEnd;
			}
			if (lastOpenTagEnd == displayText.Length - 1) break; // The last symbol is the closing tag's ']'
		}

		if (lastOpenTagName != null)
		{
			if (!Engine.IsEditorHint()) GD.PrintErr($"Opening tag '{lastOpenTagName}' without a closing tag in text '{Text}'");
			return null;
		}

		foreach (TagRange typeTag in typeTagRanges)
		{
			SetColorRange(typeTag.contentStart, typeTag.contentEnd, GetTypeColor(typeTag.tagName));
		}
		return displayText;
	}

	protected override void OnLineProcessing(int lineStart, int lineEnd, float lineStartX, float lineEndX, float ascent, float descent, Vector2 linePos, int previousParagraphsGlyphCount, Array<Dictionary> glyphs)
	{
		foreach (TagRange range in embedRanges)
		{
			int adjustedStart = range.contentStart - previousParagraphsGlyphCount;
			int adjustedEnd = range.contentEnd - previousParagraphsGlyphCount;

			if (adjustedEnd < lineStart || adjustedStart >= lineEnd) continue;

			TagRectInfo tagRect = new();
			float rectStartX, rectEndX;

			if (adjustedStart <= lineStart)
			{
				rectStartX = lineStartX;
			}
			else
			{
				rectStartX = lineStartX;
				for (int i = 0; i < glyphs.Count; i++)
				{
					var glyph = glyphs[i];
					int glyphStart = glyph["start"].As<int>();
					if (glyphStart == adjustedStart)
					{
						rectStartX = bufferGlyphPos[bufferGlyphPos.Count - glyphs.Count + i].X;
						break;
					}
				}
			}

			if (adjustedEnd >= lineEnd)
			{
				rectEndX = lineEndX;
			}
			else
			{
				rectEndX = lineEndX;
				for (int i = 0; i < glyphs.Count; i++)
				{
					var glyph = glyphs[i];
					int glyphStart = glyph["start"].As<int>();
					if (adjustedEnd == glyphStart)
					{
						rectEndX = bufferGlyphPos[bufferGlyphPos.Count - glyphs.Count + i].X;
						break;
					}
				}
			}

			tagRect.start = Math.Max(adjustedStart, lineStart) + previousParagraphsGlyphCount;
			tagRect.end = Math.Min(adjustedEnd, lineEnd) + previousParagraphsGlyphCount;
			tagRect.tagName = displayText[tagRect.start..tagRect.end];
			tagRect.rect = new Rect2(
				rectStartX - 3, // -3 and +6 are decorative padding
				linePos.Y - ascent,
				rectEndX - rectStartX + 6,
				ascent + descent
			);
			embedTagRects.Add(tagRect);
		}
	}

	protected override void OnVerticalAlignmentAdjustment(float offset)
	{
		foreach (TagRectInfo tagRect in embedTagRects)
		{
            tagRect.rect.Position += new Vector2(0, offset);
		}
	}

	protected override void PostParagraphUpdated()
	{
		foreach (Panel panel in panels)
		{
			panel.QueueFree();
		}
		panels.Clear();

		foreach (TagRectInfo tagRect in embedTagRects)
		{
			Panel panel = new()
			{
				Position = tagRect.rect.Position,
				Size = tagRect.rect.Size
			};
			panel.AddThemeStyleboxOverride("panel", embedStyle);
			AddChild(panel);
			panel.ShowBehindParent = true;
			panels.Add(panel);
		}
	}

	private class TagRange
	{
		public string tagName;
		public int contentStart;
		public int contentEnd;
	}
}