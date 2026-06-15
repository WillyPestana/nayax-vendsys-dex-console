using Microsoft.AspNetCore.SignalR;
using NayaxVendSys.Api.Hubs;
using NayaxVendSys.Application.Abstractions.Realtime;

namespace NayaxVendSys.Api.Realtime;

public sealed class SignalRDexProcessingNotifier(IHubContext<DexProcessingHub> hubContext) : IDexProcessingNotifier
{
    public Task NotifyAsync(string eventName, string message, CancellationToken cancellationToken)
    {
        var payload = new
        {
            eventName,
            message,
            occurredAtUtc = DateTimeOffset.UtcNow
        };

        return hubContext.Clients.All.SendAsync("dexProcessingEvent", payload, cancellationToken);
    }
}
