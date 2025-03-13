using Godot;

namespace MafiaHostAssistant;

public partial class NL_NightSeparator : NightLog
{
    [Export] private Label label;

    public void SetUp(uint nightCount)
    {
        label.Text = Tr("TK:NIGHT") + $" {nightCount}";
    }
}
