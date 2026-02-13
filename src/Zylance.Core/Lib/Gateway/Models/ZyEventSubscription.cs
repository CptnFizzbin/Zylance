namespace Zylance.Core.Lib.Gateway.Models;

/// <summary>
///     Represents an active event listener subscription.
/// </summary>
internal record ZyEventSubscription : Subscription
{
    public required string EventName { get; init; }
    public required Action<ZyEvent> Handler { get; init; }
}
