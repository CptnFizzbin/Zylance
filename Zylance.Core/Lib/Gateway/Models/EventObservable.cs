using Zylance.Core.Lib.Gateway.Services;

namespace Zylance.Core.Lib.Gateway.Models;

/// <summary>
///     Provides a fluent, LINQ-style API for observing events from the Gateway.
///     Supports chaining operations like Select, Where, and FirstAsync.
/// </summary>
public class EventObservable
{
    private readonly string _eventName;
    private readonly GatewayService _gatewayService;
    private readonly Func<ZyEvent, bool>? _predicate;

    internal EventObservable(GatewayService gatewayService, string eventName, Func<ZyEvent, bool>? predicate = null)
    {
        _gatewayService = gatewayService;
        _eventName = eventName;
        _predicate = predicate;
    }

    /// <summary>
    ///     Filters events based on a predicate.
    /// </summary>
    public EventObservable Where(Func<ZyEvent, bool> predicate)
    {
        Func<ZyEvent, bool> combinedPredicate = _predicate is null
            ? predicate
            : evt => _predicate(evt) && predicate(evt);

        return new EventObservable(_gatewayService, _eventName, combinedPredicate);
    }

    /// <summary>
    ///     Subscribes to events matching the current filters.
    /// </summary>
    /// <param name="handler">Action to invoke when a matching event is received.</param>
    /// <returns>A subscription that can be used to unsubscribe.</returns>
    public Subscription Subscribe(Action<ZyEvent> handler)
    {
        var wrappedHandler = _predicate is null
            ? handler
            : evt =>
            {
                if (_predicate(evt))
                    handler(evt);
            };

        return _gatewayService.SubscribeToEvent(_eventName, wrappedHandler);
    }

    /// <summary>
    ///     Waits for the first event matching the current filters.
    /// </summary>
    public Task<ZyEvent> FirstAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tcs = new TaskCompletionSource<ZyEvent>();

            var subscription = Subscribe(zyEvent => { tcs.TrySetResult(zyEvent); });

            tcs.Task.ContinueWith(_ => subscription.Unsubscribe(), TaskScheduler.Default);

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => { tcs.TrySetCanceled(cancellationToken); });

            return tcs.Task;
        }
        catch (Exception exception)
        {
            return Task.FromException<ZyEvent>(exception);
        }
    }

    /// <summary>
    ///     Projects each event to a new form.
    /// </summary>
    public EventObservable<TResult> Select<TResult>(Func<ZyEvent, TResult> selector)
    {
        return new EventObservable<TResult>(_gatewayService, _eventName, _predicate, selector);
    }
}

/// <summary>
///     Typed version of EventObservable that supports projection results.
/// </summary>
public class EventObservable<TResult>
{
    private readonly string _eventName;
    private readonly GatewayService _gatewayService;
    private readonly Func<ZyEvent, bool>? _predicate;
    private readonly Func<ZyEvent, TResult> _selector;

    internal EventObservable(
        GatewayService gatewayService,
        string eventName,
        Func<ZyEvent, bool>? predicate,
        Func<ZyEvent, TResult> selector
    )
    {
        _gatewayService = gatewayService;
        _eventName = eventName;
        _predicate = predicate;
        _selector = selector;
    }

    /// <summary>
    ///     Filters events based on the projected value.
    /// </summary>
    public EventObservable<TResult> Where(Func<TResult, bool> predicate)
    {
        Func<ZyEvent, bool> combinedPredicate = _predicate is null
            ? evt =>
            {
                try
                {
                    return predicate(_selector(evt));
                }
                catch
                {
                    return false;
                }
            }
            : evt =>
            {
                if (!_predicate(evt))
                    return false;

                try
                {
                    return predicate(_selector(evt));
                }
                catch
                {
                    return false;
                }
            };

        return new EventObservable<TResult>(_gatewayService, _eventName, combinedPredicate, _selector);
    }

    /// <summary>
    ///     Subscribes to projected values matching the current filters.
    /// </summary>
    /// <param name="handler">
    ///     Action to invoke with the projected value when a matching
    ///     event is received.
    /// </param>
    /// <returns>A subscription that can be used to unsubscribe.</returns>
    public Subscription Subscribe(Action<TResult> handler)
    {
        var wrappedHandler = new Action<ZyEvent>(evt =>
        {
            if (_predicate is not null && !_predicate(evt))
                return;

            try
            {
                var result = _selector(evt);
                handler(result);
            }
            catch
            {
                // Skip events that fail projection
            }
        });

        return _gatewayService.SubscribeToEvent(_eventName, wrappedHandler);
    }

    /// <summary>
    ///     Waits for the first projected value matching the current filters.
    /// </summary>
    public Task<TResult> FirstAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tcs = new TaskCompletionSource<TResult>();

            var subscription = Subscribe(result => { tcs.TrySetResult(result); });

            tcs.Task.ContinueWith(_ => subscription.Unsubscribe(), TaskScheduler.Default);

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => { tcs.TrySetCanceled(cancellationToken); });

            return tcs.Task;
        }
        catch (Exception exception)
        {
            return Task.FromException<TResult>(exception);
        }
    }
}
