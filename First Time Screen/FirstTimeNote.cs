using Godot;
using System;
using System.Net;

namespace MafiaHostAssistant;

public partial class FirstTimeNote : Control
{
    [Export] private FirstTimeNote previous;
    [Export] private FirstTimeNote next;

    public override void _Ready()
    {
        if (previous == null)
        {
            Position = Position with { Y = (GetViewportRect().Size.Y - Size.Y) / 2 };
        }
        else
        {
            Position = Position with { Y = GetViewportRect().Size.Y };
        }
    }

    public void ToNext() // Button
    {
        HideUp();
        next?.MoveToCenter();
    }

    public void ToPrevious() // Button
    {
        HideDown();
        previous?.MoveToCenter();
    }

    public void MoveToCenter()
    {
        // If the user changes language, the note may be bigger than needed;
        // this will make it so that the minimum size will fix it.
        Size = Size with { Y = 0 }; 
        CreateTween().TweenProperty(this, "position:y", (GetViewportRect().Size.Y - Size.Y) / 2, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }

    public void HideUp()
    {
        CreateTween().TweenProperty(this, "position:y", -Size.Y, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }

    public void HideDown()
    {
        CreateTween().TweenProperty(this, "position:y", GetViewportRect().Size.Y, 0.2d).SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.OutIn);
    }
}
