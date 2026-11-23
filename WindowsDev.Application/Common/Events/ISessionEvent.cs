namespace WindowsDev.Application.Common.Events;

public interface ISessionEvent
{
    event EventHandler? SessionExpired;
    void NotifySessionExpired();
}
