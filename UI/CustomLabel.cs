using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MafiaHostAssistant;

// Based on "interact_paragraph.gd" by Fadi-git
[Tool]
public partial class CustomLabel : Control
{
	public class ColorRange
	{
		public int start;
		public int end;
		public Color color;
	}

	[Export]
	public int FontSize
	{
		get => fontSize;
		set
		{
			fontSize = value;
			TryUpdateParagraph();
		}
	}

	[Export(PropertyHint.MultilineText)]
	public string Text
	{
		get => text;
		set
		{
			text = value;
			TryUpdateParagraph();
		}
	}

	[Export]
	public HorizontalAlignment HorizontalAlignment
	{
		get => horAlignment;
		set
		{
			horAlignment = value;
			TryUpdateParagraph();
		}
	}

	[Export]
	public VerticalAlignment VerticalAlignment
	{
		get => verAlignment;
		set
		{
			verAlignment = value;
			TryUpdateParagraph();
		}
	}

	[Export]
	public float LineSpacing
	{
		get => lineSpacing;
		set
		{
			lineSpacing = value;
			TryUpdateParagraph();
		}
	}

	private readonly List<ColorRange> colorRanges = new();
	private Font font;
	private int fontSize;
	protected Color defColor;
	private string text;
	private HorizontalAlignment horAlignment = HorizontalAlignment.Left;
	private VerticalAlignment verAlignment = VerticalAlignment.Top;
	private float lineSpacing = 0f;

	private bool isReady;
	private TextServer ts;
	private Rid fontID;
	private float height;

	protected readonly List<Vector2> bufferGlyphPos = new();
	protected readonly List<int> bufferGlyphIdx = new();
	protected readonly List<Color> bufferGlyphColor = new();
	protected readonly List<int> bufferGlyphStart = new();

	public override void _Ready()
	{
		// TODO: Subscribe to Settings' ThemeChanged event to update paragraph with new colors;
		// Remember to access Settings.Instance carefuly, as the Settings may not be loaded yet
		isReady = true;
		ts = TextServerManager.GetPrimaryInterface();
		ItemRectChanged += () => CallDeferred(MethodName.UpdateParagraph);

		StringName variation = Get(Control.PropertyName.ThemeTypeVariation).As<StringName>();
		if (variation.IsEmpty)
		{
			variation = null;
		}

		font = GetThemeFont("font", variation);
		fontID = font.GetRids()[0];

		if (Engine.IsEditorHint())
		{
			defColor = new Color("2d2d2d");
		}
		else
		{
			Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);
			Settings.Instance.Locale.Subscribe(this, OnLocaleChanged);

			defColor = Settings.Instance.IsLightTheme.Value ? Cache.Instance.LightThemeTextColor : Cache.Instance.DarkThemeTextColor;
		}

		UpdateParagraph();
	}

	public void TryUpdateParagraph()
	{
		if (isReady)
		{
			UpdateParagraph();
		}
	}

	private void OnThemeChanged(bool newIsLightTheme)
	{
		defColor = newIsLightTheme ? Cache.Instance.LightThemeTextColor : Cache.Instance.DarkThemeTextColor;

		TryUpdateParagraph();
	}

	private void OnLocaleChanged(string _)
	{
		TryUpdateParagraph();
	}

	private void UpdateParagraph()
	{
		bufferGlyphPos.Clear();
		bufferGlyphIdx.Clear();
		bufferGlyphColor.Clear();
		bufferGlyphStart.Clear();
		ClearColorRanges();

		if (font == null || fontSize == 0) return;

		string displayText = OnUpdateParagraph();
		if (displayText == null)
		{
			return;
		}

		List<string> paragraphs = displayText.Split('\n').ToList();
		float totalHeight = 0;
		int previousParagraphsGlyphCount = 0;

		for (int paragraphIndex = 0; paragraphIndex < paragraphs.Count; paragraphIndex++)
		{
			TextParagraph paragraph = new();
			paragraph.AddString(paragraphs[paragraphIndex], font, fontSize);
			paragraph.Width = Size.X;
			paragraph.Alignment = horAlignment;
			paragraph.BreakFlags = TextServer.LineBreakFlag.WordBound | TextServer.LineBreakFlag.Adaptive;

			int linesCount = paragraph.GetLineCount();
			float paragraphHeight = lineSpacing * (linesCount - 1);

			for (int lineIndex = 0; lineIndex < paragraph.GetLineCount(); lineIndex++)
			{
				Rid line = paragraph.GetLineRid(lineIndex);
				paragraphHeight += (float)(ts.ShapedTextGetAscent(line) + ts.ShapedTextGetDescent(line));
			}

			Vector2 paragraphPos = new(0, totalHeight);
			Vector2 linePos = paragraphPos;

			for (int lineIndex = 0; lineIndex < paragraph.GetLineCount(); lineIndex++)
			{
				Rid line = paragraph.GetLineRid(lineIndex);
				float ascent = (float)ts.ShapedTextGetAscent(line);
				float descent = (float)ts.ShapedTextGetDescent(line);
				float lineWidth = (float)ts.ShapedTextGetWidth(line);

				linePos.Y += ascent;

				var glyphs = ts.ShapedTextGetGlyphs(line);
				Vector2 glyphPos = linePos;

				switch (horAlignment)
				{
					case HorizontalAlignment.Center:
						glyphPos.X = (Size.X - lineWidth) / 2f;
						break;
					case HorizontalAlignment.Right:
						glyphPos.X = Size.X - lineWidth;
						break;
					default:
						break;
				}

				int lineStart = glyphs.Count > 0 ? glyphs[0]["start"].As<int>() : 0;
				int lineEnd = glyphs.Count > 0 ? glyphs[^1]["end"].As<int>() : 0;
				float lineStartX = glyphPos.X;
				float lineEndX = glyphPos.X + lineWidth;
				
				for (int iGlyph = 0; iGlyph < glyphs.Count; iGlyph++)
				{
					var glyph = glyphs[iGlyph];
					int glyphStart = glyph["start"].As<int>() + previousParagraphsGlyphCount;

					bufferGlyphPos.Add(glyphPos);
					bufferGlyphIdx.Add(glyph["index"].As<int>());
					bufferGlyphColor.Add(new Color(GetCharColor(glyphStart).ToRgba32()));
					bufferGlyphStart.Add(glyphStart);

					glyphPos.X += glyph["advance"].As<float>();
				}

				OnLineProcessing(lineStart, lineEnd, lineStartX, lineEndX, ascent, descent, linePos, previousParagraphsGlyphCount, glyphs);

				linePos.Y += descent + lineSpacing;
			}

			totalHeight += paragraphHeight + (paragraphIndex < paragraphs.Count - 1 ? lineSpacing : 0);
			previousParagraphsGlyphCount += paragraphs[paragraphIndex].Length;
		}

		height = totalHeight;

		if (verAlignment != VerticalAlignment.Top)
		{
			float offset = verAlignment == VerticalAlignment.Center ?
				(Size.Y - height) / 2f :
				(Size.Y - height);

			for (int i = 0; i < bufferGlyphPos.Count; i++)
			{
				var pos = bufferGlyphPos[i];
				pos.Y += offset;
				bufferGlyphPos[i] = pos;
			}

			OnVerticalAlignmentAdjustment(offset);
		}

		UpdateMinimumSize();

		PostParagraphUpdated();

		QueueRedraw();
	}
	
	/// <summary>
	/// Called after the text is verified to be not null. Must return the modified displayText or null if the text is invalid
	/// </summary>
	protected virtual string OnUpdateParagraph() { return text; }

	/// <summary>
	/// Called when UpdateParagraph processes a line
	/// </summary>
	protected virtual void OnLineProcessing(int lineStart, int lineEnd, float lineStartX, float lineEndX, float ascent, float descent, Vector2 linePos, int previousParagraphsGlyphCount, Godot.Collections.Array<Godot.Collections.Dictionary> glyphs) { }

	/// <summary>
	/// Called if verAlignment != VerticalAlignment.Top right before the minimum size is updated.
	/// </summary>
	protected virtual void OnVerticalAlignmentAdjustment(float offset) { }

	/// <summary>
	/// Called after minimum size is set, but before the redraw is queued
	/// </summary>
	protected virtual void PostParagraphUpdated() { }

	protected Color GetTypeColor(string typeName)
	{
		bool isLightTheme = Settings.Instance.IsLightTheme.Value; // TODO: check settings.
		if (isLightTheme)
		{
			return typeName switch
			{
				"bool" => Colors.CornflowerBlue,
				"int" => Colors.Goldenrod,
				"string" => new Color("f3683d"),
				"player" => Colors.DarkGreen,
				"union" => Colors.DarkCyan,
				"list" => Colors.DarkOrchid,
				"null" => new Color("284457"),
				_ => defColor
			};
		}
		return typeName switch
		{
			"bool" => Colors.CornflowerBlue,
			"int" => new Color("b5cea8"),
			"string" => Colors.DarkSalmon,
			"player" => Colors.LightSeaGreen,
			"union" => Colors.MediumTurquoise,
			"list" => Colors.DarkOrchid,
			"null" => new Color("C6D4E0"),
			_ => defColor
		};
	}

	public override void _Draw()
	{
		for (int i = 0; i < bufferGlyphIdx.Count; i++)
		{
			ts.FontDrawGlyph(fontID, GetCanvasItem(), fontSize, bufferGlyphPos[i], bufferGlyphIdx[i], bufferGlyphColor[i]);
		}
	}

	public void SetColorRange(int start, int end, Color color)
	{
		bool alreadyExists = false;
		foreach (ColorRange colorRange in colorRanges)
		{
			if (colorRange.start == start && colorRange.end == end)
			{
				colorRange.color = color;
				alreadyExists = true;
				break;
			}
		}
		if (!alreadyExists)
		{
			colorRanges.Add(new ColorRange()
			{
				start = start,
				end = end,
				color = color
			});
		}
		for (int i = 0; i < bufferGlyphColor.Count; i++)
		{
			if (bufferGlyphStart[i] >= start && bufferGlyphStart[i] <= end)
			{
				bufferGlyphColor[i] = color;
			}
		}
	}

	public void ClearColorRanges()
	{
		colorRanges.Clear();
		for (int i = 0; i < bufferGlyphColor.Count; i++)
		{
			bufferGlyphColor[i] = defColor;
		}
	}

	private Color GetCharColor(int charPos)
	{
		foreach (ColorRange colorRange in colorRanges)
		{
			if (charPos >= colorRange.start && charPos <= colorRange.end)
			{
				return colorRange.color;
			}
		}
		return defColor;
	}

	public override Vector2 _GetMinimumSize()
	{
		if (font == null || fontSize == 0 || !isReady) return Vector2.Zero;
		return new Vector2(0, height);
	}
}