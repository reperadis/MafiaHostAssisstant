using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class Settings : Control
{
	public static Settings Instance { get; private set; }
	
	// TODO: Instance is now guaranteed to be set in Bootstrap,
	// remove awaiting this from everywhere, except for BootstrapManager
	public static readonly TaskCompletionSource InstanceSet = new();

	private Settings() // So that the Settings is loaded even if initaly is not visible
	{
		Instance = this;
		CallDeferred(MethodName.Initialise);
	}

	[Export] private Dropdown themeSettingDropdown;
	[Export] private Dropdown languageSettingsDropdown;
	[Export] private Texture2D ruFlagTexture;
	[Export] private Texture2D ukFlagTexture;

	private Window root;

	private void Initialise()
	{
        root = GetTree().Root;
		GetParent().RemoveChild(this);
		root.AddChild(this);
		root.ChildEnteredTree += OnSceneChanged;
		
		// TODO: Find a way to detect theme change

		languageSettingsDropdown.AddElements(GetLanguageSettingOptions());

		themeSettingDropdown.AddElements(new List<Dropdown.ElementData>()
		{
			new (null, Tr("TK:LIGHT-THEME")),
			new (null, Tr("TK:DARK-THEME")),
			new (null, Tr("TK:AUTO-THEME"))
		});

		if (!File.Exists(FilePaths.GetSettingsSaveFilePath()))
		{
			InstanceSet.SetResult();
			return;
		}
		
		ImportSettings();

		InstanceSet.SetResult();
	}
	
	public void ImportSettings()
	{
		SettingsSave save = JsonConvert.DeserializeObject<SettingsSave>(File.ReadAllText(FilePaths.GetSettingsSaveFilePath()));

		SetHoldDuration(save.holdDuration);
		// UI is updated by the Singal bound in the editor
		languageSettingsDropdown.Current = LocaleToIndex(save.locale);
		themeSettingDropdown.Current = (int)save.themeMode;
	}

	public List<Dropdown.ElementData> GetLanguageSettingOptions()
	{
		return new List<Dropdown.ElementData>(){
			new (
				ruFlagTexture,
				"Русский"
			),
			new (
				ukFlagTexture,
				"English"
			)
		};
	}

	public static int LocaleToIndex(string locale)
	{
		return locale switch
		{
			"ru" => 0,
			"en" => 1,
			_ => 1 // Hoping that the user knows English :)
		};
	}

	private void OnSceneChanged(Node _)
    {
        root.MoveChild(this, -1);
    }

	public void SetLocale(int localeIndex)
	{
        string locale = IndexToLocale(localeIndex);
        TranslationServer.SetLocale(locale);
		Locale.SetLoud(locale);
        // TODO: Very possibly not the only thing that needs to be done.
		// Custom Labels, such as the ones in the Documantion, have to be recomputed.

        static string IndexToLocale(int index)
		{
			return index switch
			{
				0 => "ru",
				1 => "en",
				_ => OS.GetLocaleLanguage()
			};
		}
	}

    public SettingsValue<string> Locale = new();
	public SettingsValue<float> HoldDuration = new();
	public SettingsValue<bool> IsLightTheme = new(); // TODO: Check if this is not confused with Godot's Node.ThemChanged anywhere
	private ThemeMode themeMode;

	[Export] private Label holdDurationLabel;

	public void SetHoldDuration(float duration)
	{
		HoldDuration.SetLoud(duration);
		holdDurationLabel.Text = duration.ToString();
	}

	public void SetThemeMode(int modeIndex)
	{
		ThemeMode mode = modeIndex switch
		{
			0 => ThemeMode.Light,
			1 => ThemeMode.Dark,
			_ => ThemeMode.Auto
		};
		themeMode = mode;
		if (mode == ThemeMode.Auto)
		{
			IsLightTheme.SetLoud(!DisplayServer.IsDarkMode());
		}
		else
		{
			IsLightTheme.SetLoud(mode == ThemeMode.Light);
		}
	}
	
	[Export] private Control settingsTab;
	[Export] private Control generalDocsTab;
	[Export] private Control behaviorEditorDocsTab;
	
	public void SwitchToSettings()
	{
		settingsTab.Visible = true;
		generalDocsTab.Visible = false;
		behaviorEditorDocsTab.Visible = false;
	}
	
	public void SwtichToGeneralDocs()
	{
		settingsTab.Visible = false;
		generalDocsTab.Visible = true;
		behaviorEditorDocsTab.Visible = false;
	}
	
	public void SwtichToBehaviorEditorDocs()
	{
		settingsTab.Visible = false;
		generalDocsTab.Visible = false;
		behaviorEditorDocsTab.Visible = true;
	}
}

public class SettingsValue<T>
{
	private T value;
	private event Action<T> ValueChanged;
	private readonly ConditionalWeakTable<Node, Subscriber> subscribers = new();

	public T Value
	{
		get => value;
		private set => this.value = value;
	}

	public void Subscribe(Node node, Action<T> handler)
	{
		if (!subscribers.TryGetValue(node, out var subscriber))
		{
			subscriber = new Subscriber(this, node);
			subscribers.Add(node, subscriber);
		}

		ValueChanged += handler;
		subscriber.Add(handler);
	}

	public void SetLoud(T value)
	{
		Value = value;
		ValueChanged?.Invoke(value);
	}

	public void SetSilent(T value)
	{
		Value = value;
	}

	private class Subscriber
	{
		private readonly SettingsValue<T> parent;
		private readonly Node node;
		private readonly List<Action<T>> handlers = new();

		public Subscriber(SettingsValue<T> parent, Node node)
		{
			this.parent = parent;
			this.node = node;
			this.node.TreeExiting += Unsubscribe;
		}

		public void Add(Action<T> handler)
		{
			handlers.Add(handler);
		}

		private void Unsubscribe()
		{
			node.TreeExiting -= Unsubscribe;

			foreach (var handler in handlers)
			{
				parent.ValueChanged -= handler;
			}

			handlers.Clear();
			parent.subscribers.Remove(node);
		}
	}
}

[Serializable]
public class SettingsSave
{
	[JsonRequired]
	public const ushort ObjectVersion = 1;
	public float holdDuration;
	public ThemeMode themeMode;
	public string locale;

    public SettingsSave(float holdDuration, ThemeMode themeMode, string locale)
    {
        this.holdDuration = holdDuration;
        this.themeMode = themeMode;
        this.locale = locale;
    }
}

public class SettingsSubscriptionsHandler
{
	private readonly Node _owner;
	private readonly List<Action> unsubscribeActions = new();

	public SettingsSubscriptionsHandler(Node owner)
	{
		_owner = owner;
		_owner.TreeExiting += Dispose;
	}

	public void AddSubscription(Action subscribe, Action unsubscribe)
	{
		subscribe();
		unsubscribeActions.Add(unsubscribe);
	}

	public void Dispose()
	{
		if (_owner != null)
		{
			_owner.TreeExiting -= Dispose;
			foreach (var unsubscribe in unsubscribeActions)
			{
				unsubscribe();
			}
			unsubscribeActions.Clear();
		}
	}
}

public enum ThemeMode
{
	Light,
	Dark,
	Auto
}

public static class Translator
{
	public static string TryTranslateRoleName(string name)
	{
		if (!name.StartsWith('@'))
		{
			return name;
		}

		bool isEn = Settings.Instance.Locale.Value == "en";
		return name switch
		{
			"@Mafia" => isEn ? "Mafia" : "Мафия",
			_ => name
		};
	}

	public static string TryGetRoleDescription(string roleName)
	{
		if (!roleName.StartsWith('@'))
		{
			return string.Empty;
		}

		bool isEn = Settings.Instance.Locale.Value == "en";
		return roleName switch
		{
			"@Mafia" => isEn ? "Mafia" : "Мафия", // TODO: Make up description
			_ => string.Empty
		};
	}

	public static string TryTranslateBehaviorName(string name)
	{
		if (!name.StartsWith('@'))
		{
			return name;
		}

		bool isEn = Settings.Instance.Locale.Value == "en";
		return name switch
		{
			"@AlwaysWake" => isEn ? "Always wake" : "Просыпаться всегда",
			_ => name
		};
	}

	public static string TryTranslateBehaviorVariableName(string name)
	{
		if (!name.StartsWith('@'))
		{
			return name;
		}

		bool isEn = Settings.Instance.Locale.Value == "en";
		return name switch
		{
			"@True" => isEn ? "@True" : "@Истина",
			"@False" => isEn ? "@False" : "@Ложь",
			"@Null" => isEn ? "@Null" : "@Ничего",
			_ => name
		};
	}
}