using Godot;

namespace MafiaHostAssistant;

public partial class OpenSettingsButton : Button
{
    private Settings settings;

    public override void _Ready()
    {
        settings = Settings.Instance;
    }

    public override void _Pressed()
    {
        settings.Visible = true;
    }
}
