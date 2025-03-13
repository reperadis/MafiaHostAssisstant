using Godot;
using System;
using System.IO;

namespace MafiaHostAssistant;

public partial class BootstrapManager : Control
{
    public override async void _Ready()
    {
        // Wait for settings to detach itself from this scene
        await Settings.InstanceSet.Task;
        
        if (!IsInsideTree())
        {
            return;
        }
        
        await Cache.InstanceSet.Task;
        await Settings.InstanceSet.Task;
        
        if (!File.Exists(FilePaths.GetStructureVersionFlagFilePath()) || ushort.Parse(File.ReadAllText(FilePaths.GetStructureVersionFlagFilePath())) < FirstTimeScreenManager.StructureVersion)
        {
            GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/First Time Screen.tscn");
            return;
        }

        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/Menu.tscn");
    }
}
