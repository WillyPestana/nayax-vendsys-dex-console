namespace NayaxVendSys.Application.Abstractions.Realtime;

public interface IDexProcessingNotifier
{
    Task NotifyAsync(string eventName, string message, CancellationToken cancellationToken);
}
