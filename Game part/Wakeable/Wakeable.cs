using System.Threading.Tasks;

namespace MafiaHostAssistant;

public abstract class Wakeable
{
    public abstract Task Initialize();

    /// <summary>
    /// Determines whether this wakeable should wake this night
    /// </summary>
    /// <returns>True if the Wakeable should wake, false if the game manager should skip to the next entry</returns>
    public abstract Task<bool> CanExecute(WakeableHandlerWindow handler);

    /// <summary>
    /// Create fields if needed
    /// </summary>
    public abstract void PrepareExecution(WakeableHandlerWindow handler);

    /// <summary>
    /// Check whether all fields are populated properly
    /// </summary>
    /// <returns>True if all required fields are properly configured and execution can proceed</returns>
    public abstract bool CanProceedToExecution();

    /// <summary>
    /// Executes the wakeable's special behavior
    /// </summary>
    public abstract Task ProceedToExecution();

    /// <summary>
    /// Is called whenever the user chooses to skip this wakeable before ProceedToExecution is called
    /// </summary>
    public abstract void CancelBeforeExecution();
}