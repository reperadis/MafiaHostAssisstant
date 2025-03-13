using System.Collections.Generic;
using Godot;

namespace MafiaHostAssistant;

public abstract partial class NightLog : Control
{
    /*public abstract void AddNote((string, bool)[] evaluatedNote);
    public abstract NightLogSave GetLogSave();

    public abstract class NightLogSave
    {
    
    }*/ // TODO: Adding notes is the same for every log type

    public void AddNotes(List<List<SharedDynamicStringElementData>> messages)
    {
        
    }
}

public abstract partial class CausationChainLog : NightLog
{
    [Export] private Control causationChainContent;
    [Export] private PackedScene causationArrowScene; // TODO: This *WILL* be a transparent image of an arrow pointing down

    public void AddSubLog(NightLog log)
    {
        causationChainContent.AddChild(causationArrowScene.Instantiate());
        causationChainContent.AddChild(log);
    }
}