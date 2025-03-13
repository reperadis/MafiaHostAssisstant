using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class DynamicStringDisplayer : CustomLabel
{
	private class VariableEmbed
	{
		public string playerName; // Null if not a player
		public int start;
		public int end;
		public Color color;
		public List<Rect2> rects = new();
	}

	[Export] private PackedScene linkedHButtonScene;

	private StyleBoxFlat embedStyle;

	private readonly List<VariableEmbed> varEmbeds = new();

	protected readonly Dictionary<int, List<LinkedHButton>> linkedButtonGroups = new();
	private readonly List<Panel> embedPanels = new();
	private List<DisplayableDynamicStringElementData> elements;

	public override void _Ready()
	{
		bool isLightTheme = true; // TODO: check settings.
		embedStyle = new StyleBoxFlat()
		{
			BgColor = new Color(isLightTheme ? "e9e9e9" : "2c3e50"), // TODO: Find the best looking color
			CornerRadiusTopLeft = 8,
			CornerRadiusTopRight = 8,
			CornerRadiusBottomLeft = 8,
			CornerRadiusBottomRight = 8
		};
		base._Ready();
	}

	public void SetText(List<DisplayableDynamicStringElementData> elements)
	{
		if (this.elements != null)
		{
			foreach (DisplayableDynamicStringElementData element in this.elements)
			{
				element.StringSource.OnStringChanged -= TryUpdateParagraph;
			}
		}

		this.elements = elements;

		foreach (DisplayableDynamicStringElementData element in elements)
		{
			element.StringSource.OnStringChanged += TryUpdateParagraph;
		}

		TryUpdateParagraph();
	}

	protected override string OnUpdateParagraph()
	{
		string displayText = string.Empty;
		varEmbeds.Clear();
		
		foreach (KeyValuePair<int, List<LinkedHButton>> pair in linkedButtonGroups)
		{
			foreach (LinkedHButton button in pair.Value)
			{
				button.QueueFree();
			}
		}
		linkedButtonGroups.Clear();
		
		foreach (Panel panel in embedPanels)
		{
			panel.QueueFree();
		}
		embedPanels.Clear();
		
		if (elements == null)
		{
			return null;
		}
		
		foreach (DisplayableDynamicStringElementData element in elements)
		{
			if (!element.isVariable)
			{
				displayText += element.StringSource.GetString();
			}
			else if (element.variableType == BehaviorVariableType.Player)
			{
				string playerName = element.StringSource.GetString();
				varEmbeds.Add(new VariableEmbed
				{
					playerName = playerName,
					start = displayText.Length - displayText.Count("\n", 0, displayText.Length),
					end = displayText.Length + playerName.Length - displayText.Count("\n", 0, displayText.Length) - 1, // -1 to account for zero-based indexing
					color = GetTypeColor(TypeToTagString(element.variableType))
				});
				displayText += playerName;
			}
			else
			{
				// TODO: Figure out the lists. I guess they should work the same way as if a single player variable was added at a time
				// like so: "arsonist ignites: [player1], [Veni], [another player name]...", where [string] is an embedded player name.
				// The text, as above, should be passed to the displayer in a form of multiple dynamic string elements (even if it originates from one variable):
				// one for each element of the list, and one for each comma and space between them.
				string str = element.StringSource.GetString();

				varEmbeds.Add(new VariableEmbed
				{
					playerName = null,
					start = displayText.Length - displayText.Count("\n", 0, displayText.Length),
					end = displayText.Length + str.Length - displayText.Count("\n", 0, displayText.Length) - 1, // -1 to account for zero-based indexing
					color = GetTypeColor(TypeToTagString(element.variableType))
				});

				displayText += str;
			}
		}
		return displayText;
	}


	protected override void OnLineProcessing(int lineStart, int lineEnd, float lineStartX, float lineEndX, float ascent, float descent, Vector2 linePos, int previousParagraphsGlyphCount, Godot.Collections.Array<Godot.Collections.Dictionary> glyphs)
	{
		foreach (VariableEmbed embed in varEmbeds)
		{
			int adjustedStart = embed.start - previousParagraphsGlyphCount;
			int adjustedEnd = embed.end - previousParagraphsGlyphCount;
			// Skip if range ends before this line or starts after this line
			if (adjustedEnd < lineStart || adjustedStart >= lineEnd)
				continue;

			float rectStartX, rectEndX;

			// If range starts before this line, rect starts at line start
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

			// If range ends after this line, rect ends at line end
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
						rectEndX = bufferGlyphPos[bufferGlyphPos.Count - glyphs.Count + i].X + glyph["advance"].As<float>();
						break;
					}
				}
			}

			embed.rects.Add(new Rect2(
				rectStartX,
				linePos.Y - ascent,
				rectEndX - rectStartX,
				ascent + descent
			));
		}
	}

	protected override void OnVerticalAlignmentAdjustment(float offset)
	{
		foreach (VariableEmbed embed in varEmbeds)
		{
			List<Rect2> newRects = new();
			foreach (Rect2 rect in embed.rects)
			{
				newRects.Add(rect with {Position = Position + new Vector2(0, offset)});
			}
		}
	}

	protected override void PostParagraphUpdated()
	{
		string previousPlayerName = null;
		Dictionary<int, string> indexNameMap = new();
		int currentGroup = -1;
		foreach (VariableEmbed embed in varEmbeds)
		{
			if (embed.playerName != null)
			{
				if (previousPlayerName != embed.playerName)
				{
					currentGroup++;
					previousPlayerName = embed.playerName;
				}
				
				foreach (Rect2 rect in embed.rects)
				{
					LinkedHButton button = linkedHButtonScene.Instantiate<LinkedHButton>();
					AddChild(button);
					button.Position = rect.Position;
					button.Size = rect.Size;
					button.ShowBehindParent = true;
					if (linkedButtonGroups.ContainsKey(currentGroup))
					{
						linkedButtonGroups[currentGroup].Add(button);
					}
					else
					{
						linkedButtonGroups.Add(currentGroup, new List<LinkedHButton> { button });
						indexNameMap.Add(currentGroup, embed.playerName);
					}
				}
			}
			else
			{
				foreach (Rect2 rect in embed.rects)
				{
					Panel panel = new()
					{
						Position = rect.Position,
						Size = rect.Size
					};
					panel.AddThemeStyleboxOverride("panel", embedStyle);
					embedPanels.Add(panel);
					AddChild(panel);
					panel.ShowBehindParent = true;
				}
			}
		}

		float totalDuration = 4; // TODO: Check settings
		for (int i = 0; i < linkedButtonGroups.Count; i++)
		{
			List<LinkedHButton> group = linkedButtonGroups[i];
			float totalWidth = 0;
			foreach (LinkedHButton button in group)
			{
				totalWidth += button.Size.X;
			}
			for (int j = 0; j < group.Count; j++)
			{
				LinkedHButton button = group[j];
				float duration = button.Size.X / totalWidth * totalDuration;
				if (j != group.Count - 1)
				{
					int jCopy = j; // Otherwise j is not freezed
					button.SetUp(this, i, duration, () => group[jCopy + 1].StartFilling());
				}
				else
				{
					string playerName = indexNameMap[i];
					button.SetUp(this, i, duration, () =>
					{
						OnPlayerButtonHeld(playerName);
					});
				}
			}
		}
	}

	protected virtual void OnPlayerButtonHeld(string playerName) { }
	
	public void StartFillingFirstInGroup(int groupIndex)
	{
		linkedButtonGroups[groupIndex][0].StartFilling();
	}
	
	public void StopAllButtonsFromGroup(int groupIndex)
	{
		foreach (var button in linkedButtonGroups[groupIndex])
		{
			button.PointerUpManual();
		}
	}

	private static string TypeToTagString(BehaviorVariableType type)
	{
		return type switch
		{
			BehaviorVariableType.Bool => "bool",
			BehaviorVariableType.Integer => "int",
			BehaviorVariableType.String => "string",
			BehaviorVariableType.Player => "player",
			BehaviorVariableType.Union => "union",
			BehaviorVariableType.ListOfBools => "list",
			BehaviorVariableType.ListOfInts => "list",
			BehaviorVariableType.ListOfStrings => "list",
			BehaviorVariableType.ListOfPlayers => "list",
			_ => null
		};
	}
}
