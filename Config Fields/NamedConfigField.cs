using Godot;

namespace MafiaHostAssistant;
public partial class NamedConfigField : Control
{
    [Export] private Theme lightTheme;
    [Export] private Theme darkTheme;

    public override void _Ready()
    {
        Theme = Settings.Instance.IsLightTheme.Value ? lightTheme : darkTheme;
        Settings.Instance.IsLightTheme.Subscribe(this, OnThemeChanged);
    }

    private void OnThemeChanged(bool newIsLightTheme)
    {
        Theme = newIsLightTheme ? lightTheme : darkTheme;
    }
}
