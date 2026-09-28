namespace Application.Automation;

/// <summary>
/// Platform-neutral navigation seam. Implemented per platform so that a navigation
/// event can drive the active session without referencing platform types.
/// </summary>
public interface INavigator
{
    Task NavigateAsync(string url);
}
