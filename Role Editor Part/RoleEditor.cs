using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace MafiaHostAssistant;

public partial class RoleEditor : Control
{
    private static string EditedRoleName;
    private static bool IsEditing;
    [Export] private PackedScene roleActionPanelScene;
    [Export] private Button toPageOneButton;
    [Export] private HoldableButton cancelButton;
    [Export] private Button toPageTwoButton;
    [Export] private HoldableButton createButton;
    
    [Export] private Control pageOne;
    [Export] private Control pageTwo;

    [Export] private NamedStringConfigField roleNameField;
    [Export] private NamedStringConfigField roleDescriptionField;
    
    [Export] private BehaviorSelectionPanel passivePlayerActionPanel;
    [Export] private BehaviorSelectionPanel passiveUnionActionPanel;
    [Export] private Control activeActionPanelsContent;
    
    public readonly List<VariableRecord> createdVariables = new();
    
    public event Action OnCreatedVariablesAdded;
    
    private string roleName;
    private string roleDescription;

    public static void SetEditData(string editedRoleName)
    {
        IsEditing = true;
        EditedRoleName = editedRoleName;
    }

    public override void _Ready()
    {
        roleNameField.SetUp("TK:ROLE-NAME", string.Empty, StringFieldContext.FileName, false, SetRoleName);
        roleDescriptionField.SetUp("TK:ROLE-DESCRIPTION", string.Empty, StringFieldContext.Unrestricted, true, SetRoleDescription);
        passivePlayerActionPanel.SetUp(Tr("TK:PASSIVE-ACTION-WITH-PLAYER"), BehaviorType.PassiveActionWithPlayer, this);
        passiveUnionActionPanel.SetUp(Tr("TK:PASSIVE-ACTION-WITH-UNION"), BehaviorType.PassiveActionWithUnion, this);

        if (IsEditing)
        {
            // TODO: Import
        }
    }


    public void SetRoleName(string name)
    {
        roleName = name;
    }
    
    public void SetRoleDescription(string description)
    {
        roleDescription = description;
    }

    public void MoveToPageOne()
    {
        Tween tween1 = pageOne.CreateTween();
        tween1.TweenProperty(pageOne, "position:x", -pageOne.Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
        Tween tween2 = pageTwo.CreateTween();
        tween2.TweenProperty(pageTwo, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }

    public void ExitEditor() // Is also called by a button
    {
        GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
        IsEditing = false;
        EditedRoleName = null;
    }

    public void MoveToPageTwo()
    {
        Tween tween1 = pageOne.CreateTween();
        tween1.TweenProperty(pageOne, "position:x", 0, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
        Tween tween2 = pageTwo.CreateTween();
        tween2.TweenProperty(pageTwo, "position:x", GetViewportRect().Size.X, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }
    
    public void AddActiveAction()
    {
        RoleActionPanel panel = roleActionPanelScene.Instantiate<RoleActionPanel>();
        panel.SetUp(this, activeActionPanelsContent.GetChildCount() + 1);
        activeActionPanelsContent.AddChild(panel);
    }
    
    public void AddCreatedVariables(VariableRecord[] variables)
    {
        createdVariables.AddRange(variables);
        OnCreatedVariablesAdded?.Invoke();
    }
    
    public void TryCreateRole()
    {
        if (!passivePlayerActionPanel.IsValid() || !passiveUnionActionPanel.IsValid() || string.IsNullOrWhiteSpace(roleName))
        {
            return;
        }

        if (IsEditing)
        {
            if (EditedRoleName != roleName)
            {
                File.Delete(Path.Combine(FilePaths.GetRolesDirectoryPath(), EditedRoleName + ".json"));
            }
        }
        else
        {
            if (File.Exists(Path.Combine(FilePaths.GetRolesDirectoryPath(), roleName + ".json")))
            {
                // TODO: Display a warning
                return;
            }
        }

        List<RoleActionRecord> roleActions = new();
        foreach (Node child in activeActionPanelsContent.GetChildren())
        {
            RoleActionPanel panel = (RoleActionPanel)child;
            if (!panel.IsValid())
            {
                return;
            }
            roleActions.Add(panel.Read());

            // TODO: What to do if the role has its WakingAlgorythm stripped by WakeableUnion?
            // If it happens, the shared variables created by the algorythm will not exist in the game.
            // Ideas for fix: Prohibit the role (or better yet, action that has a WA that creates variables) from being a part of a Union; Prohibit WakingAlgorythm
            // from creating shared variables; Retrun default values for created variables when requested.
        }

        RoleRecord roleRecord = new(
            roleName: roleName,
            roleDescription: roleDescription,
            activeActions: roleActions,
            passivePlayerActionPanel.Read(),
            passiveUnionActionPanel.Read()
        );

        File.WriteAllText(Path.Combine(FilePaths.GetRolesDirectoryPath(), roleName + ".json"), JsonConvert.SerializeObject(roleRecord));
        ExitEditor();
    }
}