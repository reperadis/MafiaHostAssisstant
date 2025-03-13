using Godot;
using System;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class Cache : Node
{
    public static Cache Instance { get; private set; }
    public static readonly TaskCompletionSource InstanceSet = new();
    private Window root;
    public Cache()
    {
        Instance = this;
        CallDeferred(MethodName.Initialise);
    }
    
    private async void Initialise()
    {
        root = GetTree().Root;
        GetParent().RemoveChild(this);
        root.AddChild(this);
        root.ChildEnteredTree += OnSceneChanged;
        
        await Settings.InstanceSet.Task;
        Settings.Instance.IsLightTheme.Subscribe(this, RecacheVariableTypeTextures);
        RecacheVariableTypeTextures(Settings.Instance.IsLightTheme.Value);
        InstanceSet.SetResult();
    }
    
    private void OnSceneChanged(Node _)
    {
        root.MoveChild(this, -1);
    }

    public readonly Color DarkThemeTextColor = new("e8e8e8");
    public readonly Color LightThemeTextColor = new("2d2d2d");

    [Export] public PackedScene NamedBoolFieldScene;
    [Export] public PackedScene NamedIntFieldScene;
    [Export] public PackedScene NamedStringFieldScene;
    [Export] public PackedScene NamedListBoolField;
    [Export] public PackedScene NamedListIntField;
    [Export] public PackedScene NamedListStringField;

    public Color ErroringRed { get; private set; } = new Color("c62828");
    
    private Texture2D boolTypeTexture;
    private Texture2D intTypeTexture;
    private Texture2D stringTypeTexture;
    private Texture2D playerTypeTexture;
    private Texture2D unionTypeTexture;
    private Texture2D listBoolTypeTexture;
    private Texture2D listIntTypeTexture;
    private Texture2D listStringTypeTexture;
    private Texture2D listPlayerTypeTexture;
    private Texture2D nullTypeTexture;
    
    private void RecacheVariableTypeTextures(bool isLightTheme)
    {
        // TODO: Create the textures, figure out the colors
        boolTypeTexture = GD.Load<Texture2D>(isLightTheme ? "res://Graphics/Types/Light/Light Bool.png" : "res://Graphics/Types/Dark/Dark Bool.png");
        intTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light Int.png" : "res://Graphics/Types/Dark/Dark Int.png");
        stringTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light String.png" : "res://Graphics/Types/Dark/Dark String.png");
        playerTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light Player.png" : "res://Graphics/Types/Dark/Dark Player.png");
        unionTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light Union.png" : "res://Graphics/Types/Dark/Dark Union.png");
        listBoolTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light List Bool.png" : "res://Graphics/Types/Dark/Dark List Bool.png");
        listIntTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light List Int.png" : "res://Graphics/Types/Dark/Dark List Int.png");
        listStringTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light List String.png" : "res://Graphics/Types/Dark/Dark List String.png");
        listPlayerTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light List Player.png" : "res://Graphics/Types/Dark/Dark List Player.png");
        nullTypeTexture = (Texture2D)GD.Load(isLightTheme ? "res://Graphics/Types/Light/Light Null.png" : "res://Graphics/Types/Dark/Dark Null.png");
    }
    
    public Texture2D GetVariableTypeTexture(BehaviorVariableType type)
    {
        return type switch
        {
            BehaviorVariableType.Bool => boolTypeTexture,
            BehaviorVariableType.Integer => intTypeTexture,
            BehaviorVariableType.String => stringTypeTexture,
            BehaviorVariableType.Player => playerTypeTexture,
            BehaviorVariableType.Union => unionTypeTexture,
            BehaviorVariableType.ListOfBools => listBoolTypeTexture,
            BehaviorVariableType.ListOfInts => listIntTypeTexture,
            BehaviorVariableType.ListOfStrings => listStringTypeTexture,
            BehaviorVariableType.ListOfPlayers => listPlayerTypeTexture,
            BehaviorVariableType.Nothing => nullTypeTexture,
            _ => null
        };
    }
}
