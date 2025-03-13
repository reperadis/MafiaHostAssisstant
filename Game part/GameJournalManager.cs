using Godot;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public partial class GameJournalManager : Control
{
    [Export] private PackedScene nlPlayerNoVictimScene;
    [Export] private PackedScene nlPlayerVsPlayersScene;
    [Export] private PackedScene nlUnionHeadScene;
    [Export] private PackedScene nlCombinationHeadScene;
    [Export] private PackedScene noteScene;
    [Export] private PackedScene nightSeparatorScene;
    [Export] private Control logsContent;
    [Export] private GameStateManager gameManager;

    /*[Export] private GameObject addLogRackButton;*/
    private NightLog currentLog; // TODO: What is this for?
    private readonly List<List<SharedDynamicStringElementData>> logNotesQueue = new();
    private readonly Stack<CausationChainLog> causationStack = new();

    public void CreatePlayerNoVictimLog(Player acter, string displayName)
    {
        NL_PlayerNoVictim log = nlPlayerNoVictimScene.Instantiate<NL_PlayerNoVictim>();
        log.SetUp(acter, displayName, gameManager);
        if (causationStack.Count != 0)
        {
            causationStack.Peek().AddSubLog(log);
        }
        else
        {
            logsContent.AddChild(log);
        }
        currentLog = log;
    }

    public void CreatePlayerVsPlayersLog(Player acter, string displayName, List<Player> players)
    {
        NL_PlayerVsPlayers log = nlPlayerVsPlayersScene.Instantiate<NL_PlayerVsPlayers>();
        log.SetUp(acter, displayName, players, gameManager);
        if (causationStack.Count != 0)
        {
            causationStack.Peek().AddSubLog(log);
        }
        else
        {
            logsContent.AddChild(log);
        }
        currentLog = log;
    }

// TODO: Whenever a log is created, check if not null and if so, create the log in there, but create an arrow before it

    public void CreateUnionHeaderLog(ActionUnion union)
    {
        NL_UnionHead log = nlPlayerVsPlayersScene.Instantiate<NL_UnionHead>();
        log.SetUp(union);
        causationStack.Push(log);
        if (causationStack.Count != 0)
        {
            causationStack.Peek().AddSubLog(log);
        }
        else
        {
            logsContent.AddChild(log);
        }
        currentLog = log;
    }

    public void CloseCausation()
    {
        causationStack.Pop();
    }

    /*public void CreateUnionNoVictimLog(ActionUnion union, RoleAction action)
    {
        NL_UnionNoVictims log = nlUnionNoVictimScene.Instantiate<NL_UnionNoVictims>();
        logsContent.AddChild(log);
        log.SetUp(union, action, gameManager);
        currentLog = log;
    }

    public void CreateUnionVsPlayersLog(ActionUnion union, RoleAction action, List<Player> targets)
    {
        NL_UnionVsPlayers log = nlUnionVsPlayersScene.Instantiate<NL_UnionVsPlayers>();
        logsContent.AddChild(log);
        log.SetUp(union, action, targets, gameManager);
        log.AddNotes(logNotesQueue);
        logNotesQueue.Clear();
    }*/ // TODO: Now unions do not act by themselves. The logs for this new logic should look like this: "Union {Union}" -> {Log for subwakeable};

    public void OpenNightLogJournal()
    {
        Visible = true;
        /*addLogRackButton.SetActive(true);*/
        CreateTween().TweenProperty(this, "position:x", 0, 0.2f).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }

    public void CloseNightLogJournal()
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "position:x", GetViewportRect().Size.X, 0.2f).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
        tween.TweenCallback(Callable.From(Finish));
        /*addLogRackButton.SetActive(false);*/
        void Finish()
        {
            Visible = false;
        }
    }
    
    public void CreateNightSeparator(uint nightCount)
    {
        NL_NightSeparator separator = nightSeparatorScene.Instantiate<NL_NightSeparator>();
        logsContent.AddChild(separator);
        separator.SetUp(nightCount);
    }

    public void QueueLogNote(List<SharedDynamicStringElementData> message) // Flushed at the moment of creation
    {
        logNotesQueue.Add(message);
    }

    /*public List<NightLog.NightLogSave> GetJournalSave() // TODO
    {
        List<NightLog.NightLogSave> res = new();

        foreach (Transform log in content)
        {
            if (log.TryGetComponent(out NightLog comp))
            {
                res.Add(comp.GetLogSave());
            }
            else
            {
                res.Add(null);
            }
        }
        return res;
    }*/
}