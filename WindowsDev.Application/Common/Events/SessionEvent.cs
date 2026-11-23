namespace WindowsDev.Application.Common.Events;

internal class SessionEvent : ISessionEvent
{
    public event EventHandler? SessionExpired;

    public void NotifySessionExpired()
    {
        var handler = SessionExpired;
        handler?.Invoke(this, EventArgs.Empty);
    }
}
