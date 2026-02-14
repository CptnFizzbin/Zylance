namespace Zylance.Core.Gateway.Models;

/// <summary>
///     Represents an active subscription to a gateway event; disposing the
///     subscription unsubscribes it.
/// </summary>
public record Subscription : IDisposable
{
    /// <summary>
    ///     Identifier of the subscription.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Action to execute to unsubscribe.
    /// </summary>
    public required Action Unsubscribe { get; init; }

    /// <summary>
    ///     Disposes the subscription by invoking the unsubscribe action.
    /// </summary>
    public void Dispose()
    {
        Unsubscribe();
        GC.SuppressFinalize(this);
    }
}
