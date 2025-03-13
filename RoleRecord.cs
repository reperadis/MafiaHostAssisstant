using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

[Serializable]
public class RoleRecord
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public string roleName;
	public string roleDescription;

	public List<RoleActionRecord> activeActions;

	public ConfiguredBehaviorRecord passivePlayerActionRef; // Against players
	public ConfiguredBehaviorRecord passiveUnionActionRef; // Against unions

    public RoleRecord(string roleName, string roleDescription, List<RoleActionRecord> activeActions, ConfiguredBehaviorRecord passivePlayerActionRef, ConfiguredBehaviorRecord passiveUnionActionRef)
    {
        this.roleName = roleName;
        this.roleDescription = roleDescription;
        this.activeActions = activeActions;
        this.passivePlayerActionRef = passivePlayerActionRef;
        this.passiveUnionActionRef = passiveUnionActionRef;
    }
}

[Serializable]
public class ConfiguredBehaviorRecord // This is a part of RoleRecord, it is not stored independently
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public string behaviorName; // Not path
	public Dictionary<string, object> config; //The Configurable Variables

    public ConfiguredBehaviorRecord(string behaviorName, Dictionary<string, object> config)
    {
        this.behaviorName = behaviorName;
        this.config = config;
    }

    public BehaviorRecord ToBehaviorRecord(BehaviorType behaviorType)
	{
		BehaviorRecord actionRecord = JsonConvert.DeserializeObject<BehaviorRecord>(File.ReadAllText(Path.Combine(behaviorType.GetFolderPath(), behaviorName + ".json")), GSSC.GSS);
		return actionRecord;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(behaviorName, config);
	}

}

[Serializable]
// This is the thing that stores the logic of a behavior in JSON
public class BehaviorRecord
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public VariableRecord[] ConfigurableVariables;
	public VariableRecord[] CreatedSharedVariables;
	public VariableRecord[] AccessedSharedVariables;
	public string[] tags;
	public bool leavesNoTrace; // True if sequences have no Global or Shared variables and therefore, no execution affects the latter executions
	public bool createsLog;

	public List<OperationReference> IniSequence;
	public List<OperationReference> MainSequence;
	public BehaviorType CIBehaviorType;
}

public enum BehaviorType
{
	ActiveActionWithPlayer,
	PassiveActionWithPlayer,
	WakingAlgorythm,
	PassiveActionWithUnion
}

[Serializable]
public class OperationReference
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public OperationName OperationName;
	public Arguments Argumens;

	public OperationReference(OperationName operationName, Arguments argumens)
	{
		OperationName = operationName;
		Argumens = argumens;
	}

	[Serializable]
	public abstract class Arguments { }
}

public enum OperationName // TODO: Refer to this to not forget to implement anything
{
	IfStatement,
	RequestRoute,
	CreatePersistentVariable,
	CreateConfig,
	CreateSharedVariable,
	CreateVariable,
	ReadSharedVariable,
	AskValue,
	AssignDirectly,
	AttachLogNote,
	BinaryOperation,
	CompareEquality,
	CompareIntegers,
	ControlFlow,
	LogicalOperation,
	FindAllPlayersWithRole,
	ForeachLoop,
	GetValue,
	HighlightPlayer,
	IntegerOperation,
    Branch,

}

public enum BehaviorVariableType
{
	Bool,
	Integer,
	String,
	Player,
	Union,
	ListOfBools,
	ListOfInts,
	ListOfStrings,
	ListOfPlayers,

	Anything,
	ListOfAnything,
	Nothing
}

public static class EnumExtensions
{
	public static string GetFolderPath(this BehaviorType behaviorType)
	{
		return behaviorType switch
		{
			BehaviorType.ActiveActionWithPlayer => FilePaths.GetActiveActionsWithPlayersDirectoryPath(),
			BehaviorType.PassiveActionWithPlayer => FilePaths.GetPassiveActionsWithPlayersDirectoryPath(),
			BehaviorType.PassiveActionWithUnion => FilePaths.GetPassiveActionsWithUnionsDirectoryPath(),
			BehaviorType.WakingAlgorythm => FilePaths.GetWakingAlgorythmsDirectoryPath(),
			_ => null
		};
	}
	
	public static Texture2D GetTypeTexture(this BehaviorVariableType variableType)
	{
		return Cache.Instance.GetVariableTypeTexture(variableType);
	}

	public static bool IsList(this BehaviorVariableType variableType)
	{
		return variableType switch
		{
			BehaviorVariableType.ListOfBools or BehaviorVariableType.ListOfInts or BehaviorVariableType.ListOfPlayers or BehaviorVariableType.ListOfAnything => true,
			_ => false
		};
	}

	public static string ToTranslatedFormatedStringLowercase(this BehaviorVariableType variableType)
	{
		bool isEnglish = TranslationServer.GetLocale() == "en";
		return variableType switch
		{
			BehaviorVariableType.Bool => isEnglish ? "[e][bool]bool[/bool][/e]" : "[e][bool]булев[/bool][/e]",
			BehaviorVariableType.Integer => isEnglish ? "[e][int]integer[/int][/e]" : "[e][int]число[/int][/e]",
			BehaviorVariableType.String => isEnglish ? "[e][string]string[/string][/e]" : "[e][string]строка[/string][/e]",
			BehaviorVariableType.Player => isEnglish ? "[e][player]player[/player][/e]" : "[e][player]игрок[/player][/e]",
			BehaviorVariableType.Union => isEnglish ? "[e][union]union[/union][/e]" : "[e][union]объединение[/union][/e]",
			BehaviorVariableType.ListOfBools => isEnglish ? "[e][list]list[/list] of [bool]bools[/bool][/e]" : "[e][list]список[/list] [bool]булев[/bool][/e]",
			BehaviorVariableType.ListOfInts => isEnglish ? "[e][list]list[/list] of [int]ints[/int][/e]" : "[e][list]список[/list] [int]чисел[/int][/e]",
			BehaviorVariableType.ListOfStrings => isEnglish ? "[e][list]list[/list] of [string]strings[/string][/e]" : "[e][list]список[/list] [string]строк[/string][/e]",
			BehaviorVariableType.ListOfPlayers => isEnglish ? "[e][list]list[/list] of [player]players[/player][/e]" : "[e][list]список[/list] [player]игроков[/player][/e]",
			_ => null,
		};
	}
}

[Serializable]
public class VariableRecord
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public string varName;
	public BehaviorVariableType varType;

	public VariableRecord(string varName, BehaviorVariableType varType)
	{
		this.varName = varName;
		this.varType = varType;
	}

	public override bool Equals(object obj)
	{
		return obj is VariableRecord other &&
			   varName == other.varName &&
			   varType == other.varType;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(varName, varType);
	}
}

public static class GSSC
{
	// Gloabl Serializer Settings
	public static readonly JsonSerializerSettings GSS = new() { TypeNameHandling = TypeNameHandling.All };
}

public class RoleActionRecord
{
    public ConfiguredBehaviorRecord actionRef;
    public ConfiguredBehaviorRecord wakingAlgorythmRef;
    public string displayName;

    public RoleActionRecord(ConfiguredBehaviorRecord actionRef, ConfiguredBehaviorRecord wakingAlgorythmRef, string displayName)
    {
        this.actionRef = actionRef;
        this.wakingAlgorythmRef = wakingAlgorythmRef;
        this.displayName = displayName;
    }
}