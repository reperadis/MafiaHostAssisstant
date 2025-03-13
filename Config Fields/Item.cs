using Godot;
using System;

namespace MafiaHostAssistant;

// Using a class is more reliable than using Metadata
public partial class Item : Control
{
    [Export] public TextureRect icon;
    [Export] public Label label;
}
