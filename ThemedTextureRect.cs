using Godot;
using System;

namespace MafiaHostAssistant;

public partial class ThemedTextureRect : TextureRect
{
    [Export] private Texture2D lightTexture;
    [Export] private Texture2D darkTexture;

    public override void _Ready()
    {
        Texture = Settings.Instance.IsLightTheme.Value ? lightTexture : darkTexture;
        Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);
    }

    private void OnThemeChanged(bool isLightTheme)
    {
        Texture = isLightTheme ? lightTexture : darkTexture;
    }
}
