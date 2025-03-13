using Godot;
using System;
using System.Collections.Generic;

namespace MafiaHostAssistant;

public abstract partial class OPMessagingOperation : Operation
{
    [Export] private PanelContainer previewContainer;
    [Export] private DynamicStringDisplayer preview;
    [Export] private TextureRect togglePreviewButtonTR;
    [Export] private Texture2D showTexture;
    [Export] private Texture2D hideTexture;

    private int badNoteErrorIndex = -1;
    private BEDynamicStringConfigField noteField;

    protected BehaviorEditor behaviorEditor;
    protected List<BEDynamicStringElementData> message = new();

    private readonly List<Action> variableSubscriptions = new();

    protected override void OnAddition(BehaviorEditor behaviorEditor)
    {
        this.behaviorEditor = behaviorEditor;
    }

    protected void CreateMessageField()
    {
        behaviorEditor.SetConfigWindowActive();

        foreach (Action unsubscribe in variableSubscriptions)
        {
            unsubscribe();
        }
        variableSubscriptions.Clear();

        noteField = behaviorEditor.CreateBEDynamicStringField("TK:OP_FIELD_MESSAGE", ParentScope, message);

        noteField.TreeExiting += VerifyNoteVariableReferencesAndUnsub;
        if (previewContainer.Visible)
        {
            TogglePreview();
        }
    }

    public void VerifyNoteVariableReferencesAndUnsub()
    {
        if (badNoteErrorIndex != -1)
        {
            ResolveError(badNoteErrorIndex);
        }

        foreach (BEDynamicStringElementData element in message)
        {
            if (element.isVariable)
            {
                if (element.variable.IsInvalid)
                {
                    // TODO: Path is null
                    badNoteErrorIndex = PushError(null, ConstructNoteReferencesInvalidVariableErrorMessage(element.variable.TrueVariableName), true);
                    return;
                }
                BehaviorVariable capturedVariable = element.variable; // Local copy
                capturedVariable.OnVariableRemoved += Handler;
                variableSubscriptions.Add(() => capturedVariable.OnVariableRemoved -= Handler);

                void Handler() => OnAnyVariableRemoved(capturedVariable);
            }
        }
        noteField.TreeExiting -= VerifyNoteVariableReferencesAndUnsub; // To not prevent Garbage Collection
    }

    private void OnAnyVariableRemoved(BehaviorVariable variable)
    {
        if (badNoteErrorIndex != -1)
        {
            return;
        }

        badNoteErrorIndex = PushError(null, ConstructNoteReferencesInvalidVariableErrorMessage(variable.TrueVariableName), true);
    }

    public void TogglePreview()
    {
        previewContainer.Visible = !previewContainer.Visible;
        togglePreviewButtonTR.Texture = previewContainer.Visible ? hideTexture : showTexture;
        if (previewContainer.Visible)
        {
            preview.SetText(BEToDisplayable(message));
        }
    }

    protected override void OnDeletion()
    {
        foreach (Action unsubscribe in variableSubscriptions)
        {
            unsubscribe();
        }
        variableSubscriptions.Clear();
    }

    protected void WriteMessage(List<SharedDynamicStringElementData> note)
    {
        // TODO;
    }

    public static string ConstructNoteReferencesInvalidVariableErrorMessage(string variableName)
    {
        if (TranslationServer.GetLocale() == "en")
        {
            return $"The note references a deleted variable {variableName}!";
        }
        else // "ru"
        {
            return $"Заметка ссылается на удалённую переменную {variableName}!";
        }
    }
}
