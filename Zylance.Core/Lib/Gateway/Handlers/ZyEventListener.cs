using Zylance.Core.Lib.Gateway.Models;

namespace Zylance.Core.Lib.Gateway.Handlers;

public record ZyEventListener : Subscription
{
    public required string EventName { get; init; }
    public required Action<ZyEvent> Handler { get; init; }
}
