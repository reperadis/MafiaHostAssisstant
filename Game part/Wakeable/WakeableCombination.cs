using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MafiaHostAssistant;

public partial class WakeableCombination : Wakeable
{
    public List<Wakeable> ContainedWakeables { get; } = new();
    public string CombinationName { get; private set; }

    public override Task Initialize()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> CanExecute(WakeableHandlerWindow handler)
    {
        throw new NotImplementedException();
    }

    private WakeableHandlerWindow handler;

    public override void PrepareExecution(WakeableHandlerWindow handler)
    {
        this.handler = handler;
    }

    public override void CancelBeforeExecution()
    {
        throw new NotImplementedException();
    }

    public override bool CanProceedToExecution()
    {
        throw new NotImplementedException();
    }

    public override async Task ProceedToExecution()
    {
        await handler.HandleNestedWakeables(ContainedWakeables);
    }
}
