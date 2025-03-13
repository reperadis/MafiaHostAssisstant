using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class WakeableHandlerWindow : Control
{
    [Export] private Control tabButtonsContent;
    [Export] private PackedScene tabButtonScene;
    [Export] private PackedScene subWakeableButtonScene;
    [Export] private Control content;

    [Export] private PackedScene playersSelectionFieldScene;

    private GameStateManager gameManager;
    private TaskCompletionSource<bool> confirmationCompletionSource;

    public async Task HandleWakeable(Wakeable wakeable, GameStateManager gameManager)
    {
        this.gameManager = gameManager;

        if (!await wakeable.CanExecute(this))
        {
            return;
        }

        wakeable.PrepareExecution(this);

        // The loop is used to wait not for confirmation or skipping only,
        // but for skipping or populating the fields and then confirming
        while (true)
        {
            confirmationCompletionSource = new TaskCompletionSource<bool>();

            bool confirmed = await confirmationCompletionSource.Task;

            if (confirmed)
            {
                if (wakeable.CanProceedToExecution())
                {
                    await wakeable.ProceedToExecution();
                    break;
                }
                // Else, the user populated the fields improperly
            }
            else
            {
                // User chose to skip this wakeable // TODO: Clean up the fields, or maybe QueueFree()
                break;
            }
        }
    }

    public async Task HandleNestedWakeables(List<Wakeable> nestedWakeables)
    {
        List<Task> tasks = new(nestedWakeables.Count);
        foreach (Wakeable nestedWakeable in nestedWakeables)
        {
            WakeableHandlerWindow nestedHandler = new();
            content.AddChild(nestedHandler);
            tasks.Add(nestedHandler.HandleWakeable(nestedWakeable, gameManager));
        }
        await Task.WhenAll(tasks);
    }

    public async Task HandleNestedWakeable(Wakeable nestedWakeable)
    {
        WakeableHandlerWindow nestedHandler = new();
        content.AddChild(nestedHandler);
        await nestedHandler.HandleWakeable(nestedWakeable, gameManager);
    }

    private void Confirm() // Button
    {
        confirmationCompletionSource.SetResult(true);
    }

    private void Skip() // Button
    {
        confirmationCompletionSource.SetResult(false);
    }

    public void DisplayDynamicString(List<DisplayableDynamicStringElementData> message)
    {
        // TODO;
    }

    public async Task AwaitConfirmation()
    {
        confirmationCompletionSource = new();
        await confirmationCompletionSource.Task;
    }

    public void CreatePlayersSelectionField(int playerCount, string label, List<Player> current, Action receiver)
    {
        NamedPlayerSelectionField field = playersSelectionFieldScene.Instantiate<NamedPlayerSelectionField>();
        content.AddChild(field);
        field.SetUp(label, playerCount, current, gameManager, receiver);
    }

    public void CreateRouteSelectionField(string label, List<string> options, Action<int> receiver)
    {
        RouteSelectionField field = new();
        content.AddChild(field);
        field.SetUp(label, options, receiver);
    }

    public void CreateBoolField(string label, bool current, Action<bool> receiver)
    {
        NamedBoolConfigField field = new();
        content.AddChild(field);
        field.SetUp(label, current, receiver);
    }

    public void CreateIntField(string label, int current, bool allowNegative, Action<int> receiver)
    {
        NamedIntConfigField field = new();
        content.AddChild(field);
        field.SetUp(label, current, allowNegative, receiver);
    }

    public void CreateStringField(string label, string current, Action<string> receiver)
    {
        NamedStringConfigField field = new();
        content.AddChild(field);
        field.SetUp(label, current, StringFieldContext.Unrestricted, true, receiver);
    }
}
