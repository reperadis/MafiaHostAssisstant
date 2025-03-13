using Godot;
using System;

namespace MafiaHostAssistant;
public partial class RoleActionPanel : Control
{
    [Export] private StringConfigField decorativeNameField;
    [Export] private BehaviorSelectionPanel actionPanel;
    [Export] private BehaviorSelectionPanel waPanel;
    
    private RoleEditor roleEditor;
    private string decorativeName;

    public void SetUp(RoleEditor roleEditor, int count)
    {
        this.roleEditor = roleEditor;
        actionPanel.SetUp(Tr("TK:ACTION"), BehaviorType.ActiveActionWithPlayer, roleEditor);
        waPanel.SetUp(Tr("TK:WAKING-ALGORYTHM"), BehaviorType.WakingAlgorythm, roleEditor);
        decorativeName = Tr("TK:ACTION") + " " + count;
        decorativeNameField.SetUp(decorativeName, StringFieldContext.Unrestricted, false, ReceiveDecorativeName);
    }
    
    private void ReceiveDecorativeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            decorativeNameField.Text = decorativeName; // To revert it to last valid
            return;
        }
        decorativeName = name;
    }

    public bool IsValid()
    {
        return actionPanel.IsValid() && waPanel.IsValid();
    }
    
    public RoleActionRecord Read()
    {
        return new RoleActionRecord(
            actionRef: actionPanel.Read(),
            wakingAlgorythmRef: waPanel.Read(),
            displayName: decorativeName
        );
    }
}
