using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MafiaHostAssistant;

public partial class FirstTimeScreenManager : Control
{
    [Export] private Dropdown languageSelectionDropdown;
    [Export] private Theme lightTheme;
    [Export] private Theme darkTheme;
    
    public const ushort StructureVersion = 1;

    public override async void _Ready()
    {
        // Not checking Settings because their default values are not generated
        if (DisplayServer.IsDarkMode())
        {
            Theme = darkTheme;
        }
        else
        {
            Theme = lightTheme;
        }

        await Settings.InstanceSet.Task;

        if (!IsInsideTree())
        {
            return;
        }

        languageSelectionDropdown.AddElements(Settings.Instance.GetLanguageSettingOptions());
        languageSelectionDropdown.ItemSelected += Settings.Instance.SetLocale; // Subscribing before setting the default is intentional
        languageSelectionDropdown.Current = Settings.LocaleToIndex(OS.GetLocaleLanguage());
    }

    public void ProceedToMenu()
    {
        Directory.CreateDirectory(FilePaths.GetRolesDirectoryPath());
        Directory.CreateDirectory(FilePaths.GetActiveActionsWithPlayersDirectoryPath());
        Directory.CreateDirectory(FilePaths.GetPassiveActionsWithPlayersDirectoryPath());
        Directory.CreateDirectory(FilePaths.GetPassiveActionsWithUnionsDirectoryPath());
        Directory.CreateDirectory(FilePaths.GetWakingAlgorythmsDirectoryPath());
        Directory.CreateDirectory(FilePaths.GetPlayerNamePresetsDirectoryPath());
        if (!File.Exists(FilePaths.GetTagsTrackerFilePath()))
        {
            File.WriteAllText(FilePaths.GetTagsTrackerFilePath(), JsonConvert.SerializeObject(new List<string>()));            
        }
        if (!File.Exists(FilePaths.GetSettingsSaveFilePath()))
        {
            File.WriteAllText(FilePaths.GetSettingsSaveFilePath(), JsonConvert.SerializeObject(GenerateDefaultSettings(), GSSC.GSS));
        }
        File.WriteAllText(FilePaths.GetStructureVersionFlagFilePath(), StructureVersion.ToString());
        // TODO: Copy built-in roles and behaviors
        Settings.Instance.ImportSettings();
        
        GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
    }

    private static SettingsSave GenerateDefaultSettings()
    {
        return new SettingsSave(
            holdDuration: 1.1f,
            themeMode: ThemeMode.Auto,
            locale: Settings.Instance.Locale.Value
        );
    }
}
