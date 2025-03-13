using Godot;
using System.IO;

namespace MafiaHostAssistant;

public static class FilePaths
{
	public static string GetRolesDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Roles");
	}
	
	public static string GetActiveActionsWithPlayersDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Active Actions With Players");
	}

	public static string GetPassiveActionsWithPlayersDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Passive Actions With Players");
	}

	public static string GetPassiveActionsWithUnionsDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Passive Actions With Unions");
	}
	
	public static string GetWakingAlgorythmsDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Waking Algorythms");
	}

	public static string GetTagsTrackerFilePath()
	{
		return Path.Combine(OS.GetUserDataDir(), "TagsTracker") + ".json";
	}

	public static string GetPlayerNamePresetsDirectoryPath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Presets", "Player Names");
	}

	public static string GetSettingsSaveFilePath()
	{
		return Path.Combine(OS.GetUserDataDir(), "Settings") + ".json";
	}

	public static string GetStructureVersionFlagFilePath()
	{
		return Path.Combine(OS.GetUserDataDir(), "StructureVersion");
	}
}
